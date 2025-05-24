using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub.ReverseProxy;

internal static class ReverseProxyApi
{
  public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app, string? routePrefix)
  {
    var api = app;
    if (!string.IsNullOrWhiteSpace(routePrefix))
    {
      api = app.MapGroup(routePrefix);
    }

    api.MapGet("/route", GetRoute)
      .WithName("GetRoutes")
      .WithDescription("Get list of configured routes.");
    api.MapPost("/route", PostRoute)
      .WithName("AddRoute")
      .WithDescription("Appends a route to the configured list of routes.");

    api.MapGet("/cluster", GetCluster)
      .WithName("GetClusters")
      .WithDescription("Get list of configured clusters.");
    api.MapPost("/cluster", PostCluster)
      .WithName("AddCluster")
      .WithDescription("Appends a cluster to the configured list of clusters.");

    return app;
  }

  public static IResult GetRoute([FromServices] ReverseProxyApp reverseProxyApp) => Results.Json(reverseProxyApp.GetRouteConfigs());

  public static async ValueTask<IResult> PostRoute([FromServices] ReverseProxyApp reverseProxyApp, [FromBody] RouteConfig routeConfig)
  {
    await reverseProxyApp.AddRoute(routeConfig);
    return Results.Ok();
  }

  public static IResult GetCluster([FromServices] ReverseProxyApp reverseProxyApp) => Results.Json(reverseProxyApp.GetClusterConfigs());

  public static async ValueTask<IResult> PostCluster([FromServices] ReverseProxyApp reverseProxyApp, [FromBody] ClusterConfig clusterConfig)
  {
    await reverseProxyApp.AddCluster(clusterConfig);
    return Results.Ok();
  }
}
