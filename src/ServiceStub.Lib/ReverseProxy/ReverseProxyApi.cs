using Hj.ServiceStub.ReverseProxy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
    api.MapPost("/route", PostRouteAsync)
      .WithName("AddRoute")
      .WithDescription("Appends routes to the configuration.");

    api.MapGet("/cluster", GetCluster)
      .WithName("GetClusters")
      .WithDescription("Get list of configured clusters.");
    api.MapPost("/cluster", PostClusterAsync)
      .WithName("AddCluster")
      .WithDescription("Appends clusters to the configuration.");

    return app;
  }

  public static IResult GetRoute([FromServices] ReverseProxyApp reverseProxyApp) => Results.Json(reverseProxyApp.GetRouteConfigs());

  public static async ValueTask<IResult> PostRouteAsync([FromServices] ReverseProxyApp reverseProxyApp, [FromBody] RouteInputDto routeInput)
  {
    foreach (var routeConfig in routeInput.Routes)
    {
      await reverseProxyApp.AddRouteAsync(routeConfig);
    }

    return Results.Ok();
  }

  public static IResult GetCluster([FromServices] ReverseProxyApp reverseProxyApp) => Results.Json(reverseProxyApp.GetClusterConfigs());

  public static async ValueTask<IResult> PostClusterAsync([FromServices] ReverseProxyApp reverseProxyApp, [FromBody] ClusterInputDto clusterInput)
  {
    foreach (var clusterConfig in clusterInput.Clusters)
    {
      await reverseProxyApp.AddClusterAsync(clusterConfig);
    }

    return Results.Ok();
  }
}
