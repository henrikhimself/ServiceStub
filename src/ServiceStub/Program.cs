var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(7080);
  options.ListenAnyIP(7443, listenOptions =>
  {
    listenOptions.UseHttps();
  });
});

builder.Services
  .ConfigureStub(builder.Configuration)
  .ConfigureProxy(builder.Configuration);

var app = builder.Build()
  .UseStub()
  .UseProxy();

await app.RunAsync();
