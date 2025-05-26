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

  /// <summary>
  /// Maps a stub endpoint to the specified path and HTTP method, executing the provided handler function when the
  /// endpoint is invoked.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <param name="path">The relative URL path where the stub endpoint will be accessible.</param>
  /// <param name="httpMethod">The HTTP method that the stub endpoint will respond to.</param>
  /// <param name="fn">A delegate that defines the logic to execute when the stub endpoint is invoked.  The delegate receives the current
  /// <see cref="HttpContext"/> and a <see cref="CancellationToken"/> for handling the request.</param>
  /// <returns>The <see cref="WebApplication"/> instance, allowing for method chaining.</returns>
  public static WebApplication MapStub(this WebApplication app, string path, HttpMethod httpMethod, Func<HttpContext, CancellationToken, Task> fn)
    => MapStub(app, StubConstants.DefaultCollection, path, httpMethod, fn);

  /// <summary>
  /// Maps a stub endpoint to the specified path and HTTP method, executing the provided handler function when the
  /// endpoint is invoked.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <param name="collectionName">The stub collection that must be active before the mapping is used.</param>
  /// <param name="path">The relative URL path where the stub endpoint will be accessible.</param>
  /// <param name="httpMethod">The HTTP method that the stub endpoint will respond to.</param>
  /// <param name="fn">A delegate that defines the logic to execute when the stub endpoint is invoked.  The delegate receives the current
  /// <see cref="HttpContext"/> and a <see cref="CancellationToken"/> for handling the request.</param>
  /// <returns>The <see cref="WebApplication"/> instance, allowing for method chaining.</returns>
  public static WebApplication MapStub(this WebApplication app, string collectionName, string path, HttpMethod httpMethod, Func<HttpContext, CancellationToken, Task> fn)
  {
    var route = StubApp.CreateRoute(collectionName, path, httpMethod);
    var stubApp = app.Services.GetRequiredService<StubApp>();
    stubApp.ApiRoutes.Add(route, fn);
    return app;
  }
}
