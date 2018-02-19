using Crypnostic.CoinMarketCap;
using HD;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HD;

namespace Crypnostic.CoinMarketCap
{
  /// <summary>
  /// https://coinmarketcap.com/api/
  /// 
  /// TODO add periodic refresh.
  /// 
  /// CoinMarketCap's API is a bit limited at the moment,
  /// coverting to another base currency does not work as well 
  /// as on the website.
  /// </summary>
  public class CoinMarketCapAPI
  {
    readonly IRestClient restClient;
    readonly Throttle throttle;

    // TODO populate!
    internal protected readonly Dictionary<string, Coin>
      tickerLowerToCoin = new Dictionary<string, Coin>();

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
        if (coin == null)
        { // Blacklisted
          continue;
        }

        try
        {
          DateTime lastUpdated;
          if (long.TryParse(ticker.last_updated, out long secondsSince))
          {
            lastUpdated = DateTimeOffset.FromUnixTimeSeconds(secondsSince).DateTime;
          }
          else
          {
            lastUpdated = default(DateTime);
          }

          coin.coinMarketCapData = new Data.MarketCap(
            ticker.symbol,
            int.Parse(ticker.rank),
            ticker.price_btc.ToNullableDecimal(),
            ticker.price_usd.ToNullableDecimal(),
            ticker._24h_volume_usd.ToNullableDecimal(),
            ticker.market_cap_usd.ToNullableDecimal(),
            ticker.available_supply.ToNullableDecimal(),
            ticker.total_supply.ToNullableDecimal(),
            ticker.max_supply.ToNullableDecimal(),
            ticker.percent_change_1h.ToNullableDecimal(),
            ticker.percent_change_24h.ToNullableDecimal(),
            ticker.percent_change_7d.ToNullableDecimal(),
            lastUpdated);
        }
        catch
        {
          Console.WriteLine();
        }
      }
    }
  }
}
