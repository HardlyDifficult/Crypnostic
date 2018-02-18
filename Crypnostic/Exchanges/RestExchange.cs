using RestSharp;
using System;
using System.Threading.Tasks;
using HD;

namespace Crypnostic.Exchanges
{
  public abstract class RestExchange : Exchange
  {
    readonly IRestClient restClient;
    readonly Method method;

    public RestExchange(
      ExchangeMonitor exchangeMonitor,
      ExchangeName exchangeName,
      int maxRequestsPerMinute,
      string baseUrl,
      Method method = Method.GET)
      : base(exchangeMonitor, exchangeName, maxRequestsPerMinute)
    {
      restClient = new RestClient(baseUrl);
      this.method = method;
    }

    protected async Task<T> Get<T>(
      string resource,
      object jsonObject = null)
      where T : new()
    {
      await throttle.WaitTillReady();
      T result = await restClient.AsyncDownload<T>(resource, method: method, jsonObject: jsonObject);
      throttle.SetLastUpdateTime();
      return result;
    }
  }
}
