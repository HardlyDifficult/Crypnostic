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
  internal class BinanceExchange : Exchange
  {
    readonly IRestClient restClient;

    readonly BinanceClient client;

    public BinanceExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Binance)
    {
      restClient = new RestClient("https://www.binance.com");

      ApiClient api = new ApiClient(null, null);
      client = new BinanceClient(api);
    }

    protected override void LoadTickerNames()
    {
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
