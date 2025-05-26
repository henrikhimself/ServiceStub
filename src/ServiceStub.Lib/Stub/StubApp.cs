namespace Hj.ServiceStub.Stub;

internal sealed class StubApp(IOptions<StubOptions> appOptions)
{
  /// <summary>
  /// Gets map of routes to API methods.
  /// Key format is "{collection name}/{path segments}/{http method}".
  /// </summary>
  public Dictionary<string, Func<HttpContext, CancellationToken, Task>> ApiRoutes { get; } = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// Gets or sets base path of static JSON files.
  /// Directory structure is "{collection name}/{path segments}/{http method}.json".
  /// </summary>
  public string JsonBasePath { get; set; } = appOptions.Value.JsonPath ?? Path.Combine(AppContext.BaseDirectory, "Json");

  public string CurrentCollection { get; set; } = StubConstants.DefaultCollection;

  public static string CreateRoute(string collectionName, ICollection<string> pathSegments, HttpMethod httpMethod)
    => CreateRoute(collectionName, string.Join('/', pathSegments), httpMethod);

  public static string CreateRoute(string collectionName, string path, HttpMethod httpMethod)
  {
    path = path.TrimStart('/').TrimEnd('/');
    var route = string.Join('/', collectionName, path, httpMethod.Method).ToLowerInvariant();
    return route;
  }

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
    ApiRoutes.TryGetValue(route, out var fn);
    return fn;
  }
}
