namespace Hj.ServiceStub.Stub;

internal sealed class InterceptMiddleware(StubApp stubApp)
{
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    if (context.Response.HasStarted
      || await HandleUsingMinimalApiAsync(context)
      || await HandleUsingStaticFileAsync(context))
    {
      return;
    }

    await next(context);
  }

  private async Task<bool> HandleUsingMinimalApiAsync(HttpContext context)
  {
    var fn = stubApp.GetApiFunc(context);
    if (fn == null)
    {
      return false;
    }

    await fn(context, CancellationToken.None);
    return true;
  }

  private async Task<bool> HandleUsingStaticFileAsync(HttpContext context)
  {
    var filePath = stubApp.GetFilePath(context);
    if (!File.Exists(filePath))
    {
      return false;
    }

    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(await File.ReadAllTextAsync(filePath));
    return true;
  }
}
