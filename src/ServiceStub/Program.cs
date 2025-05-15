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

services.Configure<AppOptions>(builder.Configuration.GetSection("ServiceStub"));

builder.Services.AddReverseProxy()
  .LoadFromMemory(InterceptMiddleware.InterceptRoute, InterceptMiddleware.InterceptCluster)
  .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

services.AddSingleton<AppState>();
services.AddSingleton<InterceptMiddleware>();

var app = builder.Build();

var appState = app.Services.GetRequiredService<AppState>();
var interceptMiddleware = app.Services.GetRequiredService<InterceptMiddleware>();

app.Map("/index.html", () => appState.GetStatus());
app.Map("/favicon.ico", () => string.Empty);
app.MapReverseProxy(proxy =>
{
  proxy.Use((context, next) => interceptMiddleware.InvokeAsync(context, next));
});

await app.RunAsync();
