using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Hj.ServiceStub.Certificate.Strategy;

internal sealed class CertificateEcdsa : ICertificateStrategy
{
  public bool CanHandle(Type asymmetricAlgorithm) => typeof(ECDsa).IsAssignableFrom(asymmetricAlgorithm);

  public bool CanHandle(string publicKeyAlgOid) => publicKeyAlgOid == "1.2.840.10045.2.1";

  public AsymmetricAlgorithm CreateKey() => ECDsa.Create(ECCurve.NamedCurves.nistP256);

  public CertificateRequest CreateCertificateRequest(AsymmetricAlgorithm key, X500DistinguishedName distinguishedName)
  {
    var certificateRequest = new CertificateRequest(
        distinguishedName,
        (ECDsa)key,
        HashAlgorithmName.SHA256);
    return certificateRequest;
  }

  public X509SignatureGenerator GetSignatureGenerator(X509Certificate2 certificate)
    => X509SignatureGenerator.CreateForECDsa(GetKey(certificate));

  public X509Certificate2 CopyWithPrivateKey(X509Certificate2 certificate, AsymmetricAlgorithm key)
    => certificate.CopyWithPrivateKey((ECDsa)key);

  public string ExportPrivateKeyPem(X509Certificate2 certificate) => GetKey(certificate).ExportECPrivateKeyPem();

  private static ECDsa GetKey(X509Certificate2 certificate)
    => certificate.GetECDsaPrivateKey() ?? throw new InvalidOperationException("Certificate has no private key");
}
