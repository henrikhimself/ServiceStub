using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Hj.ServiceStub.Certificate.Strategy;

internal interface ICertificateStrategy
{
  bool CanHandle(Type asymmetricAlgorithm);

  /// <summary>
  /// A public key OID is an object identifier identifying the algorithm.
  /// <see href="https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-gpnap/ff1a8675-0008-408c-ba5f-686a10389adc">See more</see>.
  /// </summary>
  /// <param name="publicKeyAlgOid">The algorithm OID.</param>
  /// <returns>True if strategy matches the provided OID.</returns>
  bool CanHandle(string publicKeyAlgOid);

  AsymmetricAlgorithm CreateKey();

  CertificateRequest CreateCertificateRequest(AsymmetricAlgorithm key, X500DistinguishedName distinguishedName);

  X509SignatureGenerator GetSignatureGenerator(X509Certificate2 certificate);

  X509Certificate2 CopyWithPrivateKey(X509Certificate2 certificate, AsymmetricAlgorithm key);

  string ExportPrivateKeyPem(X509Certificate2 certificate);
}
