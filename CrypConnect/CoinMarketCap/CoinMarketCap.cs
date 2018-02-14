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
  }
}
