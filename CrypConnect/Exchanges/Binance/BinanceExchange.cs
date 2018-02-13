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
  /// </summary>
  /// <remarks>
  /// https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md
  /// HTTP 429 return code is used when breaking a request rate limit.
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
          1200)
    {
      restClient = new RestClient("https://www.binance.com");

      ApiClient api = new ApiClient(null, null);
      client = new BinanceClient(api);
    }

    protected override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();
      
      BinanceProductListJson productList =
        restClient.Get<BinanceProductListJson>("exchange/public/product");

      for (int i = 0; i < productList.data.Length; i++)
      {
        BinanceProductJson product = productList.data[i];
        AddTicker(product.baseAsset, Coin.FromName(product.baseAssetName));
        AddTicker(product.quoteAsset, Coin.FromName(product.quoteAssetName));
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      await throttle.WaitTillReady();

      IEnumerable<OrderBookTicker> tickerList = await client.GetOrderBookTicker();
      AddTradingPairs(tickerList, (OrderBookTicker ticker) =>
      {
        string baseCoinTicker = null;
        string symbolLower = ticker.Symbol.ToLowerInvariant();
        foreach (KeyValuePair<string, Coin> tickerToName in tickerLowerToCoin)
        {
          if (symbolLower.EndsWith(tickerToName.Key))
          {
            baseCoinTicker = tickerToName.Key;
          }
        }

        string quoteCoinTicker = ticker.Symbol.Substring(0, ticker.Symbol.Length - baseCoinTicker?.Length ?? 0);
       
        return (baseCoinTicker: baseCoinTicker,
          quoteCoinTicker: quoteCoinTicker,
          askPrice: ticker.AskPrice,
          bidPrice: ticker.BidPrice);
      });
    }
  }
}
