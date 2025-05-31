using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Hj.ServiceStub.Certificate.Strategy;

namespace Hj.ServiceStub.Certificate;

internal sealed class CertificateFactory(IEnumerable<ICertificateStrategy> strategies)
{
  public X509Certificate2 CreateCa(AsymmetricAlgorithm key, string subjectName)
  {
    var strategy = GetStrategy(key);

    var request = strategy.CreateCertificateRequest(key, new X500DistinguishedName(subjectName));
    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
    request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

    var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
    return certificate;
  }

  public X509Certificate2 CreateCertificate(AsymmetricAlgorithm key, X509Certificate2 ca, string subjectName, Action<SubjectAlternativeNameBuilder> configureSan)
  {
    var strategy = GetStrategy(key);

    var request = strategy.CreateCertificateRequest(key, new X500DistinguishedName(subjectName));

    var sanBuilder = new SubjectAlternativeNameBuilder();
    configureSan(sanBuilder);
    request.CertificateExtensions.Add(sanBuilder.Build());

    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
    request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], true));
    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
    request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

    var caSignatureGenerator = GetStrategy(ca).GetSignatureGenerator(ca);
    var utcNow = DateTimeOffset.UtcNow;

    var serialNumber = new byte[16];
    RandomNumberGenerator.Fill(serialNumber);

    var certificate = request.Create(
        ca.IssuerName,
        caSignatureGenerator,
        utcNow.AddDays(-1),
        utcNow.AddYears(1),
        serialNumber);

    var certificateWithKey = strategy.CopyWithPrivateKey(certificate, key);
    return certificateWithKey;
  }

  public AsymmetricAlgorithm CreateKey(string algorithmOid) => GetStrategy(algorithmOid).CreateKey();

  public CertificateRequest CreateCertificateRequest(AsymmetricAlgorithm key, X500DistinguishedName distinguishedName)
    => GetStrategy(key).CreateCertificateRequest(key, distinguishedName);

  public string ExportPrivateKeyPem(X509Certificate2 certificate)
    => GetStrategy(certificate).ExportPrivateKeyPem(certificate);

  private ICertificateStrategy GetStrategy(X509Certificate2 certificate) => GetStrategy(certificate.GetKeyAlgorithm());

  private ICertificateStrategy GetStrategy(string publicKeyAlgOid)
    => strategies.FirstOrDefault(x => x.CanHandle(publicKeyAlgOid)) ?? throw new NotSupportedException($"Algorithm oid '{publicKeyAlgOid}' is not supported");

  private ICertificateStrategy GetStrategy(AsymmetricAlgorithm key)
  {
    var keyType = key.GetType();
    return strategies.FirstOrDefault(x => x.CanHandle(keyType)) ?? throw new NotSupportedException($"Algorithm type '{keyType.Name}' is not supported");
  }
}
