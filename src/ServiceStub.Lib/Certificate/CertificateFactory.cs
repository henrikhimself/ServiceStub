using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Hj.ServiceStub.Certificate.Strategy;

namespace Hj.ServiceStub.Certificate;

internal sealed class CertificateFactory(
  ILogger<CertificateFactory> logger,
  IEnumerable<ICertificateStrategy> strategies)
{
  public X509Certificate2 CreateCa(AsymmetricAlgorithm key, string subjectName)
  {
    var strategy = GetStrategy(key);

    var request = strategy.CreateCertificateRequest(key, new X500DistinguishedName(subjectName));
    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
    request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

    var utcNow = DateTimeOffset.UtcNow;
    var validFrom = utcNow.AddDays(-1);
    var validTo = utcNow.AddYears(10);

    var ca = request.CreateSelfSigned(validFrom, validTo);
    logger.LogInformation("Creating CA, subject '{SubjectName}', valid from '{ValidFrom}', valid to '{ValidTo}', thumbprint '{Thumbprint}', serial '{SerialNumber}'", subjectName, validFrom, validTo, ca.Thumbprint, ca.SerialNumber);
    return ca;
  }

  public X509Certificate2 CreateCertificate(AsymmetricAlgorithm key, X509Certificate2 ca, string subjectName, Action<SubjectAlternativeNameBuilder> configureSan)
  {
    var strategy = GetStrategy(key);

    var request = strategy.CreateCertificateRequest(key, new X500DistinguishedName(subjectName));

    var sanBuilder = new SubjectAlternativeNameBuilder();
    configureSan(sanBuilder);
    request.CertificateExtensions.Add(sanBuilder.Build());

    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
    request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], true));
    request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

    var caSignatureGenerator = GetStrategy(ca).GetSignatureGenerator(ca);

    var utcNow = DateTimeOffset.UtcNow;
    var validFrom = utcNow.AddDays(-1);
    var validTo = utcNow.AddYears(1);

    var serialNumber = new byte[16];
    RandomNumberGenerator.Fill(serialNumber);

    logger.LogInformation("Creating certificate, subject '{SubjectName}', CA thumbprint '{Thumbprint}', CA serial '{SerialNumber}'", subjectName, ca.Thumbprint, ca.SerialNumber);
    var certificate = request.Create(
        ca.IssuerName,
        caSignatureGenerator,
        validFrom,
        validTo,
        serialNumber);

    var certificateWithKey = strategy.CopyWithPrivateKey(certificate, key);

    var pfxBytes = certificateWithKey.Export(X509ContentType.Pkcs12);
    var pfx = X509CertificateLoader.LoadPkcs12(pfxBytes, null, keyStorageFlags: X509KeyStorageFlags.Exportable);
    return pfx;
  }

  public AsymmetricAlgorithm CreateKey(string algorithmOid) => GetStrategy(algorithmOid).CreateKey();

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
