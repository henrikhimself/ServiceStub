namespace Hj.ServiceStub.Stub;

internal static class StubExtensions
{
  public static IServiceCollection ConfigureStub(this IServiceCollection services, IConfiguration configuration)
  {
    services
      .Configure<StubOptions>(configuration.GetSection("Stub"))
      .AddSingleton<StubApp>()
      .AddSingleton<InterceptMiddleware>();

    return services;
  }

  public static WebApplication UseStub(this WebApplication app)
  {
    var stubApp = app.Services.GetRequiredService<StubApp>();
    var interceptMiddleware = app.Services.GetRequiredService<InterceptMiddleware>();

    app.Map("/index.html", () => stubApp.GetStatus());

    app.Use(interceptMiddleware.InvokeAsync);

    return app;
  }
}
