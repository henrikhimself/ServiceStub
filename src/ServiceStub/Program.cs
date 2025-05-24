using System.Globalization;
using System.Reflection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.GetCultureInfo("en-US"))
    .CreateLogger();

bool isBuild = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

try
{
  WebApplication? app;

  if (isBuild)
  {
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddOpenApi();
    app = builder.Build();
    app.MapOpenApi();
    app
      .UseStubApi()
      .UseReverseProxyApi();
  }
  else
  {
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(options =>
    {
      options.ListenAnyIP(7080);
      options.ListenAnyIP(7443, listenOptions =>
      {
        listenOptions.UseHttps();
      });
    });

    IServiceCollection? services = builder.Services;
    services
      .AddSerilog(lc => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(formatProvider: CultureInfo.GetCultureInfo("en-US")));
    services
      .ConfigureStub(builder.Configuration)
      .ConfigureReverseProxy(builder.Configuration);

    app = builder.Build();
    app
      .UseStub()
      .UseStubApi("stub")
      .MapStub("/hello-from-api", HttpMethod.Get, HelloFromApi.GetAsync);
    app.UseReverseProxy()
      .UseReverseProxyApi("proxy")
      .UseBlackholeCatchAll();
  }

  await app.RunAsync();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
  await Log.CloseAndFlushAsync();
}
