using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub.ReverseProxy;

public static class BlackholeConfig
{
  public const string BlackholeId = "blackhole";

  public static IReadOnlyList<RouteConfig> Route => [
    new RouteConfig()
      {
        RouteId = BlackholeId,
        ClusterId = BlackholeId,
        Match = new RouteMatch() { Path = "/{**catch-all}", },
      }
  ];

  public static IReadOnlyList<ClusterConfig> Cluster => [
    new ClusterConfig()
      {
        ClusterId = BlackholeId,
        Destinations = new Dictionary<string, DestinationConfig>()
        {
          { BlackholeId, new DestinationConfig() { Address = "https:///", } },
        },
      }
    ];
}
