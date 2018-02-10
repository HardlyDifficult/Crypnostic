using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using CryptoExchanges.Kucoin;

namespace CryptoExchanges
{
  internal class KucoinExchange : Exchange
  {
    protected readonly IRestClient restClient;
    readonly Random random = new Random();

    public KucoinExchange()
    {
      restClient = new RestClient("https://api.kucoin.com");
    }

    public override async Task<List<TradingPair>> GetAllTradingPairs()
    {
      KucoinMarketInfo tickerList;
      // TODO move to retry logic everyone can use
      while (true)
      {
        try
        {
          tickerList = 
            restClient.Get<KucoinMarketInfo>("v1/open/tick"); 

          if (tickerList != null)
          {
            break;
          }
        }
        catch
        {
          await Task.Delay(3500 + random.Next(2000));
        }
      }

      List<TradingPair> tradingPairList = new List<TradingPair>();

      foreach (Datum ticker in tickerList.data)
      {
        try
        {
          decimal askPrice = new decimal(ticker.sell);
          decimal bidPrice = new decimal(ticker.buy);

          tradingPairList.Add(new TradingPair(
            ExchangeName.Kucoin, 
            ticker.coinTypePair, 
            ticker.coinType, 
            askPrice, 
            bidPrice));
        }
        catch { }
      }

      return tradingPairList;
    }
  }
}
