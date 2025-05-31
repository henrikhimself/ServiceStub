using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Hj.ServiceStub.Certificate;

internal sealed class CertificateApp(CertificateFactory certificateFactory)
{
  public void CreateCertificate(SelfSignedOptions options)
  {
    X509Certificate2? ca = null;
    try
    {
      if (!TryGetCa(options, out ca))
      {
        ca = CreateCa(options);
      }

      CreateCertificate(options, ca);
    }
    finally
    {
      ca?.Dispose();
    }
  }

  private static void GetCaFilePaths(SelfSignedOptions options, out string caCrtPemFilePath, out string caKeyPemFilePath, out string caPfxFilePath)
  {
    caCrtPemFilePath = Path.Combine(options.FilePath, "ca.crt.pem");
    caKeyPemFilePath = Path.Combine(options.FilePath, "ca.key.pem");
    caPfxFilePath = Path.Combine(options.FilePath, "ca.pfx");
  }

  private static bool TryGetCa(SelfSignedOptions options, [NotNullWhen(true)] out X509Certificate2? ca)
  {
    GetCaFilePaths(options, out var caCrtPemFilePath, out var caKeyPemFilePath, out _);
    if (!File.Exists(caCrtPemFilePath) || !File.Exists(caKeyPemFilePath))
    {
      ca = null;
      return false;
    }

    ca = X509Certificate2.CreateFromPemFile(caCrtPemFilePath, caKeyPemFilePath);
    return true;
  }

  private void CreateCertificate(SelfSignedOptions options, X509Certificate2 ca)
  {
    using var key = certificateFactory.CreateKey(options.AlgorithmOid);

    foreach (var dnsName in options.DnsNames)
    {
      // Each dns name gets its own certificate to support dynamicly adding routes in
      // the reverse proxy api. The input for creating a wildcard certificate is
      // then "*." subject with the implied creation of the host specific certficate.
      var isWildcard = dnsName.StartsWith('*');
      var cn = isWildcard
        ? dnsName[2..]
        : dnsName;

      using var cert = certificateFactory.CreateCertificate(key, ca, $"CN={cn}", san =>
      {
        san.AddIpAddress(IPAddress.Loopback);

        san.AddDnsName(cn);
        if (isWildcard)
        {
          san.AddDnsName(dnsName);
        }
      });

      var fileName = cn.Replace('.', '-');
      var certFilePath = Path.Combine(options.FilePath, fileName + ".crt.pem");
      var keyFilePath = Path.Combine(options.FilePath, fileName + ".key.pem");
      File.WriteAllText(certFilePath, cert.ExportCertificatePem());
      File.WriteAllText(keyFilePath, certificateFactory.ExportPrivateKeyPem(cert));
    }
  }

  private X509Certificate2 CreateCa(SelfSignedOptions options)
  {
    using var key = certificateFactory.CreateKey(options.AlgorithmOid);
    var ca = certificateFactory.CreateCa(key, options.SubjectName);

    GetCaFilePaths(options, out var caCrtPemFilePath, out var caKeyPemFilePath, out var caPfxFilePath);
    File.WriteAllText(caCrtPemFilePath, ca.ExportCertificatePem());
    File.WriteAllText(caKeyPemFilePath, certificateFactory.ExportPrivateKeyPem(ca));
    File.WriteAllBytes(caPfxFilePath, ca.Export(X509ContentType.Pfx));

    return ca;
  }
}
