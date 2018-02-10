using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  internal class BinanceExchange : Exchange
  {
    BinanceClient client;

    public BinanceExchange()
    {
      ApiClient api = new ApiClient(null, null);
      client = new BinanceClient(api);
    }

    public override async Task<List<TradingPair>> GetAllTradingPairs()
    {
      IEnumerable<OrderBookTicker> tickerList = await client.GetOrderBookTicker();
      if(tickerList == null)
      {
        return null;
      }

      List<TradingPair> tradingPairList = new List<TradingPair>();

      foreach (OrderBookTicker ticker in tickerList)
      {
        tradingPairList.Add(ticker.ToTradingPair());
      }

      return tradingPairList;
    }
  }
}
