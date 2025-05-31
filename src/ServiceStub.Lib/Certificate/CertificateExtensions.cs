using Hj.ServiceStub.Certificate.Strategy;
using Microsoft.AspNetCore.Hosting;

namespace Hj.ServiceStub.Certificate;

public static class CertificateExtensions
{
  public static IWebHostBuilder ConfigureSelfSignedCertificate(this IWebHostBuilder webHostBuilder, IConfiguration configuration)
  {
    CreateSelfSignedCertificate(configuration.GetSection("SelfSignedCertificate").Get<SelfSignedOptions>());
    return webHostBuilder;
  }

  public static IWebHostBuilder ConfigureSelfSignedCertificate(this IWebHostBuilder webHostBuilder, SelfSignedOptions? options)
  {
    CreateSelfSignedCertificate(options);
    return webHostBuilder;
  }

  private static void CreateSelfSignedCertificate(SelfSignedOptions? options)
  {
    if (options == null || string.IsNullOrWhiteSpace(options.FilePath))
    {
      return;
    }

    var fullPath = Path.GetFullPath(options.FilePath);
    if (!Directory.Exists(fullPath))
    {
      throw new InvalidOperationException($"Self signed certificate file path '{fullPath}' does not exist");
    }

    options.FilePath = fullPath;
    options.AlgorithmOid ??= "1.2.840.113549.1.1.1"; // RSA
    options.SubjectName ??= "CN=Developer Root CA";
    options.DnsNames ??= [];

    var certificateFactory = new CertificateFactory([
      new CertificateRsa(),
      new CertificateEcdsa()]);

    var app = new CertificateApp(certificateFactory);
    app.CreateCertificate(options);
  }
}
