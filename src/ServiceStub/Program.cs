using System.Globalization;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.GetCultureInfo("en-US"))
    .CreateLogger();

try
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

  var services = builder.Services;

  services
    .AddSerilog(lc => lc
      .ReadFrom.Configuration(builder.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console(formatProvider: CultureInfo.GetCultureInfo("en-US")));

  services
    .ConfigureStub(builder.Configuration)
    .ConfigureReverseProxy(builder.Configuration);

  var app = builder.Build()
    .UseStub()
    .UseStubApi("stub")
    .MapStub("/hello-from-api", HttpMethod.Get, HelloFromApi.GetAsync)
    .UseReverseProxy();

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
