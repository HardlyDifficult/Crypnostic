using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// https://github.com/etherdelta/etherdelta.github.io/blob/master/docs/API_OLD.md
  /// </remarks>
  internal class EtherDeltaExchange : Exchange
  {
    protected readonly IRestClient restClient;

    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    /// <param name="exchangeMonitor"></param>
    public EtherDeltaExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.EtherDelta, 1_000_000 / 1_440)
    {
      restClient = new RestClient("https://api.etherdelta.com");
    }

    protected override async Task LoadTickerNames()
    {
      // TODO how do we do this for EtherDelta?
      await Task.Delay(0);
    }

    protected override async Task GetAllTradingPairs()
    {
      Dictionary<string, Dictionary<string, object>> tickerList;
      while (true)
      {
        try
        {
          await throttle.WaitTillReady();
          tickerList =
            restClient.Get<Dictionary<string, Dictionary<string, object>>>(
              "returnTicker");

          if (tickerList != null)
          {
            break;
          }
        }
        catch
        {
          await Task.Delay(3500 + ExchangeMonitor.instance.random.Next(2000));
        }
      }

      AddTradingPairs(tickerList,
        (KeyValuePair<string, Dictionary<string, object>> ticker) =>
          (baseCoinTicker: "ETH",
          quoteCoinTicker: ticker.Key.GetAfter("_"),
          askPrice: Convert.ToDecimal(ticker.Value["ask"]),
          bidPrice: Convert.ToDecimal(ticker.Value["bid"])));
    }
  }
}
