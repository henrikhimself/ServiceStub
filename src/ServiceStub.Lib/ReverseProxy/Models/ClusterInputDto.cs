using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub.ReverseProxy.Models;

internal sealed class ClusterInputDto
{
  public required List<ClusterConfig> Clusters { get; set; }
}
