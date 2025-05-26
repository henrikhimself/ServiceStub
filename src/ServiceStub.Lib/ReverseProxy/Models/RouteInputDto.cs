using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub.ReverseProxy.Models;

internal sealed class RouteInputDto
{
  public required List<RouteConfig> Routes { get; set; }
}
