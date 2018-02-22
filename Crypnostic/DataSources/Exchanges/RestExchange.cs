using RestSharp;
using System;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;
using System.Net;
using Common.Logging;

namespace Crypnostic.Internal
{
  /// <summary>
  /// A simple helper as Rest requests are common when working with Exchanges.
  /// </summary>
  internal abstract class RestExchange : Exchange
  {
    static readonly ILog log = LogManager.GetLogger<RestExchange>();

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
      where T : class, new()
    {
      Debug.Assert(string.IsNullOrWhiteSpace(resource) == false);

      await throttle.WaitTillReady();

      (HttpStatusCode status, T result) = await restClient.AsyncDownload<T>(resource, 
        method: method, 
        jsonObject: jsonObject,
        cancellationToken: CrypnosticController.instance.cancellationTokenSource.Token);

      if(status != HttpStatusCode.OK)
      {
        log.Error(status);
        throttle.BackOff();
        // TODO backoff if 400's
        return null;
      }

      return result;
    }
  }
}
