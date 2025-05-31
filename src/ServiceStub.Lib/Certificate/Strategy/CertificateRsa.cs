using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Hj.ServiceStub.Certificate.Strategy;

internal sealed class CertificateRsa : ICertificateStrategy
{
  public bool CanHandle(Type asymmetricAlgorithm) => typeof(RSA).IsAssignableFrom(asymmetricAlgorithm);

  public bool CanHandle(string publicKeyAlgOid) => publicKeyAlgOid == "1.2.840.113549.1.1.1";

  public AsymmetricAlgorithm CreateKey() => RSA.Create(2048);

  public CertificateRequest CreateCertificateRequest(AsymmetricAlgorithm key, X500DistinguishedName distinguishedName)
  {
    var certificateRequest = new CertificateRequest(
      distinguishedName,
      (RSA)key,
      HashAlgorithmName.SHA256,
      RSASignaturePadding.Pkcs1);
    return certificateRequest;
  }

  public X509SignatureGenerator GetSignatureGenerator(X509Certificate2 certificate)
    => X509SignatureGenerator.CreateForRSA(GetKey(certificate), RSASignaturePadding.Pkcs1);

  public X509Certificate2 CopyWithPrivateKey(X509Certificate2 certificate, AsymmetricAlgorithm key)
    => certificate.CopyWithPrivateKey((RSA)key);

  public string ExportPrivateKeyPem(X509Certificate2 certificate) => GetKey(certificate).ExportRSAPrivateKeyPem();

  private static RSA GetKey(X509Certificate2 certificate)
    => certificate.GetRSAPrivateKey() ?? throw new InvalidOperationException("Certificate has no private key");
}
