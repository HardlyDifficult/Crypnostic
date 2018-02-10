using System;
using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Market;
using CryptoExchanges.Exchanges.Binance;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges
{
  internal class BinanceExchange : Exchange
  {
    readonly IRestClient restClient;

    readonly BinanceClient client;

    public BinanceExchange()
      : base(ExchangeName.Binance)
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

    protected override async Task<List<TradingPair>> GetAllTradingPairs()
    {
      IEnumerable<OrderBookTicker> tickerList = await client.GetOrderBookTicker();
      return AddTradingPairs(tickerList, (OrderBookTicker ticker) => 
        (baseCoin: ticker.Symbol.Substring(3),
          quoteCoin: ticker.Symbol.Substring(0, 3),
          askPrice: ticker.AskPrice,
          bidPrice: ticker.BidPrice));
    }
  }
}
