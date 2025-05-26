using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub.ReverseProxy;

internal sealed class ReverseProxyApp(
  IConfigValidator configValidator,
  InMemoryConfigProvider inMemoryConfigProvider,
  IEnumerable<IProxyConfigProvider> proxyConfigProviders)
{
  private readonly RouteConfig _blackholeRoute = new()
  {
    RouteId = ReverseProxyConstants.BlackholeId,
    ClusterId = ReverseProxyConstants.BlackholeId,
    Order = int.MaxValue,
    Match = new RouteMatch() { Path = "/{**catch-all}", },
  };

  private readonly ClusterConfig _blackholeCluster = new()
  {
    ClusterId = ReverseProxyConstants.BlackholeId,
    Destinations = new Dictionary<string, DestinationConfig>()
    {
      { ReverseProxyConstants.BlackholeId, new DestinationConfig() { Address = "https:///", } },
    },
  };

  private readonly List<RouteConfig> _routes = [];
  private readonly List<ClusterConfig> _clusters = [];

  public IReadOnlyList<RouteConfig> GetRouteConfigs() => proxyConfigProviders.SelectMany(x => x.GetConfig().Routes).ToList().AsReadOnly();

  public IReadOnlyList<ClusterConfig> GetClusterConfigs() => proxyConfigProviders.SelectMany(x => x.GetConfig().Clusters).ToList().AsReadOnly();

  public void AddBlackholeCatchAll()
  {
    _routes.Add(_blackholeRoute);
    _clusters.Add(_blackholeCluster);
    Update();
  }

  public async ValueTask AddRouteAsync(RouteConfig route)
  {
    var validationErrors = await configValidator.ValidateRouteAsync(route);
    if (validationErrors.Count > 0)
    {
      throw new AggregateException("Could not add route.", validationErrors);
    }

    if (proxyConfigProviders.Any(x => x.GetConfig().Routes.Any(y => y.RouteId == route.RouteId)))
    {
      throw new InvalidOperationException($"Route with id '{route.RouteId}' already exists.");
    }

    _routes.Add(route);
    Update();
  }

  public async ValueTask AddClusterAsync(ClusterConfig cluster)
  {
    var validationErrors = await configValidator.ValidateClusterAsync(cluster);
    if (validationErrors.Count > 0)
    {
      throw new AggregateException("Could not add cluser.", validationErrors);
    }

    if (proxyConfigProviders.Any(x => x.GetConfig().Clusters.Any(y => y.ClusterId == cluster.ClusterId)))
    {
      throw new InvalidOperationException($"Cluster with id '{cluster.ClusterId}' already exists.");
    }

    _clusters.Add(cluster);
    Update();
  }

  public void Update() => inMemoryConfigProvider.Update(_routes.AsReadOnly(), _clusters.AsReadOnly());
}
