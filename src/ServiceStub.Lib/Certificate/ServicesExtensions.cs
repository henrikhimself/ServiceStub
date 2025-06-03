using Hj.ServiceStub.Certificate.Store;
using Hj.ServiceStub.Certificate.Strategy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hj.ServiceStub.Certificate;

public static class ServicesExtensions
{
  public static void UseSelfSignedCertificate(this KestrelServerOptions options)
  {
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
      httpsOptions.ServerCertificateSelector = (context, hostName) =>
      {
        if (hostName == null)
        {
          return null;
        }

        using var scope = options.ApplicationServices.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var certificateApp = serviceProvider.GetRequiredService<CertificateApp>();
        var certificate = certificateApp.GetCertificate(hostName);
        return certificate;
      };
    });
  }

  public static IServiceCollection ConfigureSelfSignedCertificate(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<SelfSignedOptions>(configuration.GetSection("SelfSignedCertificate"));
    services.AddMemoryCache();
    services.TryAddSingleton<ICertificateStore, FileSystemStore>();
    services.AddSingleton<ICertificateStrategy, CertificateRsa>();
    services.AddSingleton<ICertificateStrategy, CertificateEcdsa>();
    services.AddSingleton<CertificateFactory>();
    services.AddSingleton<CertificateApp>();

    return services;
  }
}
