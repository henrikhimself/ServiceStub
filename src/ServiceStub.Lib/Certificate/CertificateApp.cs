using System.Net;
using System.Security.Cryptography.X509Certificates;
using Hj.ServiceStub.Certificate.Store;
using Microsoft.Extensions.Caching.Memory;

namespace Hj.ServiceStub.Certificate;

internal sealed class CertificateApp(
  ILogger<CertificateApp> logger,
  IOptionsMonitor<SelfSignedOptions> options,
  IMemoryCache memoryCache,
  ICertificateStore certificateStore,
  CertificateFactory certificateFactory)
{
  public X509Certificate2 GetCertificate(string dnsName)
  {
    var certificate = memoryCache.GetOrCreate(dnsName, entry =>
    {
      var selfSignedOptions = GetOptions();

      using var ca = GetOrCreateCa(selfSignedOptions);
      using var key = certificateFactory.CreateKey(selfSignedOptions.AlgorithmOid);

      var isWildcard = dnsName.StartsWith('*');
      var cn = isWildcard
        ? dnsName[2..]
        : dnsName;

      logger.LogInformation("Missing certificate, dns name '{DnsName}', is wildcard '{IsWildcard}'", dnsName, isWildcard);
      return certificateFactory.CreateCertificate(key, ca, $"CN={cn}", san =>
      {
        san.AddIpAddress(IPAddress.Loopback);

        san.AddDnsName(cn);
        if (isWildcard)
        {
          san.AddDnsName("*." + dnsName);
        }
      });
    });

    return certificate!;
  }

  private X509Certificate2 GetOrCreateCa(SelfSignedOptions selfSignedOptions)
  {
    var ca = certificateStore.LoadCa(selfSignedOptions);
    if (ca == null)
    {
      using var key = certificateFactory.CreateKey(selfSignedOptions.AlgorithmOid);
      ca = certificateFactory.CreateCa(key, selfSignedOptions.SubjectName);
      certificateStore.SaveCa(selfSignedOptions, ca);
    }

    return ca;
  }

  private SelfSignedOptions GetOptions()
  {
    var selfSignedOptions = options.CurrentValue;

    if (string.IsNullOrWhiteSpace(selfSignedOptions.CaFilePath))
    {
      throw new InvalidOperationException("CA file path is not set");
    }

    var fullPath = Path.GetFullPath(selfSignedOptions.CaFilePath);
    if (!Directory.Exists(fullPath))
    {
      throw new InvalidOperationException($"CA file path '{fullPath}' does not exist");
    }

    selfSignedOptions.CaFilePath = fullPath;

    if (string.IsNullOrWhiteSpace(selfSignedOptions.AlgorithmOid))
    {
      // ECDsa.
      selfSignedOptions.AlgorithmOid = "1.2.840.10045.2.1";
    }

    if (string.IsNullOrWhiteSpace(selfSignedOptions.SubjectName))
    {
      selfSignedOptions.SubjectName = "CN=ServiceStub Root CA";
    }

    return selfSignedOptions;
  }
}
