using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Hj.ServiceStub.Proxy;

internal static class ProxyExtensions
{
  public static IServiceCollection ConfigureProxy(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddHttpLogging(options =>
    {
      options.CombineLogs = true;
      options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery | HttpLoggingFields.ResponseBody;
      options.RequestBodyLogLimit = int.MaxValue;
      options.ResponseBodyLogLimit = int.MaxValue;
    });

    services.AddReverseProxy()
      .LoadFromConfig(configuration.GetSection("ReverseProxy"))
      .AddTransforms(builderContext =>
      {
        builderContext.AddRequestHeader(HeaderNames.AcceptEncoding, "identity", false);

        builderContext.AddResponseHeader(HeaderNames.CacheControl, "no-store", false);
        builderContext.AddResponseHeader(HeaderNames.Pragma, "no-cache", false);
      });

    return services;
  }

  public static WebApplication UseProxy(this WebApplication app)
  {
    app.MapReverseProxy(proxy =>
    {
      proxy.UseHttpLogging();
    });

    return app;
  }
}
