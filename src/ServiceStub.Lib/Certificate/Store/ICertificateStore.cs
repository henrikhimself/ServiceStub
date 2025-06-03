using System.Security.Cryptography.X509Certificates;

namespace Hj.ServiceStub.Certificate.Store;

internal interface ICertificateStore
{
  X509Certificate2? LoadCa(SelfSignedOptions options);

  void SaveCa(SelfSignedOptions options, X509Certificate2 ca);
}
