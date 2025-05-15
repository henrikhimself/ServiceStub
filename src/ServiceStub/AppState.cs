using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace Hj.ServiceStub;

internal sealed class AppState(IOptions<AppOptions> appOptions)
{
  public string JsonBasePath { get; set; } = appOptions.Value.JsonPath ?? Path.Combine(AppContext.BaseDirectory, "Json");

  public string CurrentCollection { get; set; } = "_default";

  public string GetStatus() => $"[READY] Current collection '{CurrentCollection}', JSON path '{JsonBasePath}'";

  public ICollection<string> GetRouteSegments(HttpContext context)
  {
    var uri = new Uri(context.Request.GetDisplayUrl());
    var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return [CurrentCollection, .. segments, context.Request.Method];
  }

  public string GetRoute(HttpContext context)
    => string.Join('/', [.. GetRouteSegments(context)]);

  public string GetFilePath(HttpContext context)
    => string.Join(Path.DirectorySeparatorChar, [JsonBasePath, .. GetRouteSegments(context)]) + ".json";

  public Func<HttpContext, CancellationToken, Task>? GetApiFunc(HttpContext context)
  {
    var route = GetRoute(context);
    if (ApiRoutes.Map.TryGetValue(route, out var fn))
    {
      return fn;
    }

    return null;
  }
}
