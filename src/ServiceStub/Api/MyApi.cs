namespace Hj.ServiceStub.Api;

internal static class MyApi
{
  public static async Task GetHelloAsync(HttpContext context, CancellationToken cancellationToken)
  {
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("""{ "Hello": "Api" }""", cancellationToken);
  }
}
