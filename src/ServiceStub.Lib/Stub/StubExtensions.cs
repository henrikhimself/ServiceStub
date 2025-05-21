using System.Diagnostics.CodeAnalysis;

namespace Hj.ServiceStub.Stub;

public static class StubExtensions
{
  /// <summary>
  /// Configures the services required for the Stub feature.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to which the Stub services will be added.</param>
  /// <param name="configuration">The <see cref="IConfiguration"/> containing the settings for the Stub feature.</param>
  /// <returns>The updated <see cref="IServiceCollection"/> with the Stub services configured.</returns>
  public static IServiceCollection ConfigureStub(this IServiceCollection services, IConfiguration configuration)
  {
    return services
      .Configure<StubOptions>(configuration.GetSection("Stub"))
      .AddSingleton<StubApp>()
      .AddSingleton<StubMiddleware>();
  }

  /// <summary>
  /// Adds the <see cref="StubMiddleware"/> to the application's request pipeline.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
  public static WebApplication UseStub(this WebApplication app)
  {
    var stubMiddleware = app.Services.GetRequiredService<StubMiddleware>();
    app.Use(stubMiddleware.InvokeAsync);
    return app;
  }

  /// <summary>
  /// Adds an API for managing the stub configuration at runtime.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <param name="routePrefix">The pattern that prefixes all routes in this group.</param>
  /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
  public static WebApplication UseStubApi(this WebApplication app, [StringSyntax("Route")] string? routePrefix = null)
  {
    StubApi.MapApi(app, routePrefix);
    return app;
  }

  public static WebApplication MapStub(this WebApplication app, string path, HttpMethod httpMethod, Func<HttpContext, CancellationToken, Task> fn)
    => MapStub(app, StubConstants.DefaultCollection, path, httpMethod, fn);

  public static WebApplication MapStub(this WebApplication app, string collectionName, string path, HttpMethod httpMethod, Func<HttpContext, CancellationToken, Task> fn)
  {
    var route = StubApp.CreateRoute(collectionName, path, httpMethod);
    var stubApp = app.Services.GetRequiredService<StubApp>();
    stubApp.ApiRoutes.Add(route, fn);
    return app;
  }
}
