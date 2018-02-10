using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges
{
  internal class EtherDeltaExchange : Exchange
  {
    protected readonly IRestClient restClient;
    readonly Random random = new Random();

    public EtherDeltaExchange()
    {
      restClient = new RestClient("https://api.etherdelta.com");
    }

    public override async Task<List<TradingPair>> GetAllTradingPairs()
    {
      Dictionary<string, Dictionary<string, object>> tickerList;
      // TODO move to retry logic everyone can use
      while (true)
      {
        try
        {
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
          await Task.Delay(3500 + random.Next(2000));
        }
      }

      List<TradingPair> tradingPairList = new List<TradingPair>();

      foreach (KeyValuePair<string, Dictionary<string, object>> ticker in tickerList)
      {
        try
        {
          string coin = ticker.Key.GetAfter("_");
          decimal askPrice = Convert.ToDecimal(ticker.Value["ask"]);
          decimal bidPrice = Convert.ToDecimal(ticker.Value["bid"]);
          
          tradingPairList.Add(new TradingPair(ExchangeName.EtherDelta, "ETH", 
            coin, askPrice, bidPrice));
        }
        catch { }
      }

      return tradingPairList;
    }
  }
}
