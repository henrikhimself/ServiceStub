using Hj.ServiceStub.Stub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Hj.ServiceStub.Stub;

internal static class StubApi
{
  public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app, string? routePrefix)
  {
    var api = app;
    if (!string.IsNullOrWhiteSpace(routePrefix))
    {
      api = app.MapGroup(routePrefix);
    }

    api.MapGet("status", GetStatus);
    api.MapPost("collection", PostCollection);

    return app;
  }

  public static IResult GetStatus([FromServices] StubApp stubApp)
    => TypedResults.Ok($"[READY] Current collection '{stubApp.CurrentCollection}', JSON path '{stubApp.JsonBasePath}'");

  public static IResult PostCollection([FromServices] StubApp stubApp, [FromBody] CollectionDto collection)
  {
    var newCurrentCollection = collection.Current;
    if (string.IsNullOrWhiteSpace(newCurrentCollection))
    {
      return TypedResults.BadRequest();
    }

    stubApp.CurrentCollection = newCurrentCollection;
    return TypedResults.Ok();
  }
}
