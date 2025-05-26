using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Hj.ServiceStub.ReverseProxy;

public static class ReverseProxyExtensions
{
  /// <summary>
  /// Configures reverse proxy services for the application.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to which the reverse proxy services will be added.</param>
  /// <param name="configuration">The <see cref="IConfiguration"/> instance containing an optional reverse proxy configuration.</param>
  /// <returns>The updated <see cref="IServiceCollection"/> with reverse proxy services configured.</returns>
  public static IServiceCollection ConfigureReverseProxy(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<ReverseProxyApp>();

    services.AddHttpLogging(options =>
    {
      options.CombineLogs = true;
      options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery | HttpLoggingFields.ResponseBody;
      options.RequestBodyLogLimit = int.MaxValue;
      options.ResponseBodyLogLimit = int.MaxValue;
    });

    services.AddSingleton<LoggerMiddleware>();

    services.AddReverseProxy()
      .LoadFromMemory([], [])
      .LoadFromConfig(configuration.GetSection("ReverseProxy"))
      .AddTransforms(builderContext =>
      {
        builderContext.AddRequestHeader(HeaderNames.AcceptEncoding, "identity", false);

        builderContext.AddResponseHeader(HeaderNames.CacheControl, "no-store", false);
        builderContext.AddResponseHeader(HeaderNames.Pragma, "no-cache", false);
      });

    return services;
  }

  /// <summary>
  /// Configures the application to use the reverse proxy.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
  /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
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

  /// <summary>
  /// Configures the application to enable the Reverse Proxy API with an optional route prefix.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
  /// <param name="routePrefix">An optional route prefix for the Reverse Proxy API. If <see langword="null"/> or empty, the API will be mapped to
  /// the root route.</param>
  /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
  public static WebApplication UseReverseProxyApi(this WebApplication app, [StringSyntax("Route")] string? routePrefix = null)
  {
    ReverseProxyApi.MapApi(app, routePrefix);
    return app;
  }

  /// <summary>
  /// Configures the application to handle all unmatched requests with a "blackhole" catch-all route.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
  /// <returns>The configured <see cref="WebApplication"/> instance, allowing for further chaining of calls.</returns>
  public static WebApplication UseBlackholeCatchAll(this WebApplication app)
  {
    var reverseProxyApp = app.Services.GetRequiredService<ReverseProxyApp>();
    reverseProxyApp.AddBlackholeCatchAll();
    return app;
  }
}
