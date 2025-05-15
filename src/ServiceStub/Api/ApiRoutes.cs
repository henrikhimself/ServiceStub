namespace Hj.ServiceStub.Api;

internal static class ApiRoutes
{
  /// <summary>
  /// Maps routes to methods for dynamic responses.
  /// Key format is {collection name}/{path segments}/{http method}.
  /// </summary>
  public static readonly Dictionary<string, Func<HttpContext, CancellationToken, Task>> Map = new(StringComparer.OrdinalIgnoreCase)
  {
      { "_default/hello-from-api/get", MyApi.GetHelloAsync },
  };
}
