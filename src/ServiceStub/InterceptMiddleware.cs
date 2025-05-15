using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub;

internal sealed class InterceptMiddleware(AppState appState)
{
  public static IReadOnlyList<RouteConfig> InterceptRoute => [
    new RouteConfig()
    {
      RouteId = nameof(InterceptMiddleware),
      ClusterId = nameof(InterceptMiddleware),
      Match = new RouteMatch() { Path = "/{**catch-all}", },
    }
  ];

  public static IReadOnlyList<ClusterConfig> InterceptCluster => [
    new ClusterConfig()
      {
        ClusterId = nameof(InterceptMiddleware),
        Destinations = new Dictionary<string, DestinationConfig>()
        {
          { nameof(InterceptMiddleware), new DestinationConfig() { Address = "https:///", } },
        },
      }
    ];

  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    if (context.Response.HasStarted
        || await HandleUsingStaticFileAsync(context)
        || await HandleUsingMinimalApiAsync(context))
    {
      return;
    }

    await next(context);
  }

  private async Task<bool> HandleUsingStaticFileAsync(HttpContext context)
  {
    var filePath = appState.GetFilePath(context);
    if (!File.Exists(filePath))
    {
      return false;
    }

    context.Response.StatusCode = 200;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(await File.ReadAllTextAsync(filePath));

    return true;
  }

  private async Task<bool> HandleUsingMinimalApiAsync(HttpContext context)
  {
    var fn = appState.GetApiFunc(context);
    if (fn == null)
    {
      return false;
    }

    await fn(context, CancellationToken.None);
    return true;
  }
}
