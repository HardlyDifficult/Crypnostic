using RestSharp;
using System;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;

namespace Crypnostic
{
  /// <summary>
  /// A simple helper as Rest requests are common when working with Exchanges.
  /// </summary>
  public abstract class RestExchange : Exchange
  {
    readonly IRestClient restClient;

    readonly Method method;

    public RestExchange(
      ExchangeName exchangeName,
      string baseUrl,
      int maxRequestsPerMinute = 60,
      TimeSpan timeBetweenAutoUpdates = default(TimeSpan),
      Method method = Method.GET)
      : base(exchangeName, maxRequestsPerMinute, timeBetweenAutoUpdates)
    {
      Debug.Assert(maxRequestsPerMinute > 0);
      Debug.Assert(string.IsNullOrWhiteSpace(baseUrl) == false);

      restClient = new RestClient(baseUrl);
      this.method = method;
    }

    protected async Task<T> Get<T>(
      string resource,
      object jsonObject = null)
      where T : new()
    {
      Debug.Assert(string.IsNullOrWhiteSpace(resource) == false);

      await throttle.WaitTillReady();

      T result = await restClient.AsyncDownload<T>(resource, 
        method: method, 
        jsonObject: jsonObject,
        cancellationToken: CrypnosticController.instance.cancellationTokenSource.Token);

      throttle.SetLastUpdateTime(); // <- kinda overkill

      return result;
    }
  }
}
