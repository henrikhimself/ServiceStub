using System.Net;

namespace Hj.ServiceStub.ReverseProxy;

public class LoggerMiddleware(ILogger<LoggerMiddleware> logger)
{
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    var proxyFeature = context.GetReverseProxyFeature();
    var route = proxyFeature.Route.Config;

    if (string.Equals(ReverseProxyConstants.BlackholeId, route.RouteId, StringComparison.OrdinalIgnoreCase))
    {
      logger.LogError("Blackhole: '{Url}'", context.Request.GetDisplayUrl());
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      await context.Response.CompleteAsync();
      return;
    }

    if (logger.IsEnabled(LogLevel.Debug))
    {
      logger.LogDebug("Route '{RouteId}', match '{Match}', path '{Path}'", route.RouteId, route.Match.Path, context.Request.GetDisplayUrl());
    }

    await next(context);
  }
}
