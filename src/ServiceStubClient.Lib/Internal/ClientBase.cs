using System.Text;

namespace Hj.ServiceStub.Internal;

public abstract class ClientBase : IClientBase
{
  protected ClientBase()
  {
  }

  public virtual Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, CancellationToken ct) => Task.CompletedTask;

  public virtual Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string ur, CancellationToken ct) => Task.CompletedTask;

  public virtual Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response, CancellationToken ct) => Task.CompletedTask;
}
