using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Hj.ServiceStub.ReverseProxy;

public static class ReverseProxyExtensions
{
  public static IServiceCollection ConfigureReverseProxy(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddHttpLogging(options =>
    {
      options.CombineLogs = true;
      options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery | HttpLoggingFields.ResponseBody;
      options.RequestBodyLogLimit = int.MaxValue;
      options.ResponseBodyLogLimit = int.MaxValue;
    });

    services.AddSingleton<LoggerMiddleware>();

    services.AddReverseProxy()
      .LoadFromMemory(BlackholeConfig.Route, BlackholeConfig.Cluster)
      .LoadFromConfig(configuration.GetSection("ReverseProxy"))
      .AddTransforms(builderContext =>
      {
        builderContext.AddRequestHeader(HeaderNames.AcceptEncoding, "identity", false);

        builderContext.AddResponseHeader(HeaderNames.CacheControl, "no-store", false);
        builderContext.AddResponseHeader(HeaderNames.Pragma, "no-cache", false);
      });

    return services;
  }

  public static WebApplication UseReverseProxy(this WebApplication app)
  {
    var loggerMiddleware = app.Services.GetRequiredService<LoggerMiddleware>();

    app.MapReverseProxy(proxyPipeline =>
    {
      proxyPipeline.UseHttpLogging();
      proxyPipeline.Use(async (context, next) => await loggerMiddleware.InvokeAsync(context, next));
    });

    return app;
  }
}
