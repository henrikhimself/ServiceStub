using Yarp.ReverseProxy.Configuration;

namespace Hj.ServiceStub.ReverseProxy;

internal class ReverseProxyApp(
  IConfigValidator _configValidator,
  InMemoryConfigProvider _inMemoryConfigProvider,
  IEnumerable<IProxyConfigProvider> _proxyConfigProviders)
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

  private readonly List<RouteConfig> _routes = new();
  private readonly List<ClusterConfig> _clusters = new();

  public IReadOnlyList<RouteConfig> GetRouteConfigs() => _proxyConfigProviders.SelectMany(x => x.GetConfig().Routes).ToList().AsReadOnly();

  public IReadOnlyList<ClusterConfig> GetClusterConfigs() => _proxyConfigProviders.SelectMany(x => x.GetConfig().Clusters).ToList().AsReadOnly();

  public void AddBlackholeCatchAll()
  {
    _routes.Add(_blackholeRoute);
    _clusters.Add(_blackholeCluster);
    Update();
  }

  public async ValueTask AddRoute(RouteConfig route)
  {
    var validationErrors = await _configValidator.ValidateRouteAsync(route);
    if (validationErrors.Count > 0)
    {
      throw new AggregateException("Could not add route.", validationErrors);
    }

    if (_proxyConfigProviders.Any(x => x.GetConfig().Routes.Any(y => y.RouteId == route.RouteId)))
    {
      throw new InvalidOperationException($"Route with id '{route.RouteId}' already exists.");
    }

    _routes.Add(route);
    Update();
  }

  public async ValueTask AddCluster(ClusterConfig cluster)
  {
    var validationErrors = await _configValidator.ValidateClusterAsync(cluster);
    if (validationErrors.Count > 0)
    {
      throw new AggregateException("Could not add cluser.", validationErrors);
    }

    if (_proxyConfigProviders.Any(x => x.GetConfig().Clusters.Any(y => y.ClusterId == cluster.ClusterId)))
    {
      throw new InvalidOperationException($"Cluster with id '{cluster.ClusterId}' already exists.");
    }

    _clusters.Add(cluster);
    Update();
  }

  public void Update() => _inMemoryConfigProvider.Update(_routes.AsReadOnly(), _clusters.AsReadOnly());
}
