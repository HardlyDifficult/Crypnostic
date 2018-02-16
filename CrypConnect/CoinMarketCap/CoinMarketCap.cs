using CrypConnect.CoinMarketCap;
using HD;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypConnect.CoinMarketCap
{
  /// <summary>
  /// https://coinmarketcap.com/api/
  /// </summary>
  public class CoinMarketCapAPI
  {
    readonly IRestClient restClient;
    readonly Throttle throttle;

    public CoinMarketCapAPI()
    {
      restClient = new RestClient("https://api.coinmarketcap.com");

      // Please limit requests to no more than 10 per minute.
      throttle = new Throttle(TimeSpan.FromMinutes(2 * 1 / 10));
    }

    public async Task Refresh()
    {
      List<CoinMarketCapTickerJson> resultList = await restClient.AsyncDownload
        <List<CoinMarketCapTickerJson>>("v1/ticker/?limit=0");

      for (int i = 0; i < resultList.Count; i++)
      {
        CoinMarketCapTickerJson ticker = resultList[i];
        Coin coin = Coin.CreateFromName(ticker.name);
        if(coin == null)
        { // Blacklisted
          continue;
        }
        coin.coinMarketCapData = ticker;
      }
    }
  }
}
