using System;
using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Market;
using CryptoExchanges.Exchanges.Binance;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;

namespace CryptoExchanges
{
  /// <summary>
  /// TODO HTTP 429 return code is used when breaking a request rate limit.
  /// </summary>
  /// <remarks>
  /// https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md
  /// </remarks>
  internal class BinanceExchange : Exchange
  {
    readonly IRestClient restClient;

    readonly BinanceClient client;

    /// <summary>
    /// Throttle:
    ///   1200 requests per minute is the stated max.
    ///   Targeting half that to avoid issues.
    /// </summary>
    public BinanceExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, 
          ExchangeName.Binance,
          TimeSpan.FromMilliseconds(.5 * TimeSpan.FromMinutes(1).TotalMilliseconds / 1200.0))
    {
      restClient = new RestClient("https://www.binance.com");

      ApiClient api = new ApiClient(null, null);
      client = new BinanceClient(api);
    }

    protected override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();

      // TODO should be async
      BinanceProductListJson productList =
        restClient.Get<BinanceProductListJson>("exchange/public/product");

      for (int i = 0; i < productList.data.Length; i++)
      {
        BinanceProductJson product = productList.data[i];
        AddTicker(product.baseAsset, product.baseAssetName);
        AddTicker(product.quoteAsset, product.quoteAssetName);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      await throttle.WaitTillReady();

      IEnumerable<OrderBookTicker> tickerList = await client.GetOrderBookTicker();
      AddTradingPairs(tickerList, (OrderBookTicker ticker) =>
      {
        string baseCoinTicker = null;
        foreach (KeyValuePair<string, string> tickerToName in tickerToFullName)
        {
          if (ticker.Symbol.EndsWith(tickerToName.Key))
          {
            baseCoinTicker = tickerToName.Key;
          }
        }
        Debug.Assert(ticker.Symbol == "123456" || baseCoinTicker != null);

        string quoteCoinTicker = ticker.Symbol.Substring(0, ticker.Symbol.Length - baseCoinTicker?.Length ?? 0);

        Debug.Assert(ticker.Symbol == "123456" || quoteCoinTicker != null);
        Debug.Assert(ticker.Symbol == "123456" || quoteCoinTicker + baseCoinTicker == ticker.Symbol);

        return (baseCoin: baseCoinTicker,
          quoteCoin: quoteCoinTicker,
          askPrice: ticker.AskPrice,
          bidPrice: ticker.BidPrice);
      });
    }
  }
}
