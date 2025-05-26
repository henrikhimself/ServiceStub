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

    api.MapGet("/collection", GetCollection)
      .WithName("GetCollection")
      .WithDescription("Get active collection name.");
    api.MapPost("/collection", PostCollection)
      .WithName("SetCollection")
      .WithDescription("Set active collection.");

    return app;
  }

  public static IResult GetCollection([FromServices] StubApp stubApp)
  {
    return Results.Json<CollectionDto>(new()
    {
      Name = stubApp.CurrentCollection,
    });
  }

  public static IResult PostCollection([FromServices] StubApp stubApp, [FromBody] CollectionDto collection)
  {
    if (string.IsNullOrWhiteSpace(collection.Name))
    {
      return Results.BadRequest();
    }

    stubApp.CurrentCollection = collection.Name;
    return Results.Ok();
  }
}
