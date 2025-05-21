namespace Hj.ServiceStub.Api;

internal static class HelloFromApi
{
  public static async Task GetAsync(HttpContext context, CancellationToken cancellationToken)
  {
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync($$"""{ "Hello": "Runtime API {{DateTime.Now}}" }""", cancellationToken);
  }
}
