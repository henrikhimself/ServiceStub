using System.Net;

namespace Hj.ServiceStub.ReverseProxy;

public class LoggerMiddleware(ILogger<LoggerMiddleware> logger)
{
  private static readonly List<string> _ignoredPaths = ["/favicon.ico"];

  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    var proxyFeature = context.GetReverseProxyFeature();
    var route = proxyFeature.Route.Config;

    if (string.Equals(ReverseProxyConstants.BlackholeId, route.RouteId, StringComparison.OrdinalIgnoreCase))
    {
      if (_ignoredPaths.Contains(context.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
      {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
      }
      else
      {
        logger.LogDebug("Route: '{Url}', unknown route", context.Request.GetDisplayUrl());
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
      }

      await context.Response.CompleteAsync();
      return;
    }

    if (logger.IsEnabled(LogLevel.Debug))
    {
      logger.LogDebug("Route '{RouteId}', match '{Match}', cluster '{ClusterId}'", route.RouteId, route.Match.Path, route.ClusterId);
    }

    await next(context);
  }
}
