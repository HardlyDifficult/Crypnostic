using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using CryptoExchanges.Kucoin;
using CryptoExchanges.Exchanges.Kucoin;

namespace CryptoExchanges
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// https://kucoinapidocs.docs.apiary.io/#
  /// </remarks>
  internal class KucoinExchange : Exchange
  {
    readonly IRestClient restClient;

    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    /// <param name="exchangeMonitor"></param>
    public KucoinExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Kucoin,
          TimeSpan.FromMilliseconds(.5 * TimeSpan.FromDays(1).TotalMilliseconds / 1_000_000))
    {
      restClient = new RestClient("https://api.kucoin.com");
    }

    protected override async Task LoadTickerNames()
    {
      throw new Exception();
      await throttle.WaitTillReady();

      KucoinTickerNameListJson productList =
        restClient.Get<KucoinTickerNameListJson>("v1/market/open/coins");

      for (int i = 0; i < productList.data.Length; i++)
      {
        KucoinTickerNameJson product = productList.data[i];
        AddTicker(product.coin, product.name);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      await throttle.WaitTillReady();

      KucoinMarketInfo tickerList =
            restClient.Get<KucoinMarketInfo>("v1/open/tick");
      AddTradingPairs(tickerList.data, (KucoinTradingPairJson ticker) =>
        (baseCoin: ticker.coinTypePair,
          quoteCoin: ticker.coinType,
          askPrice: new decimal(ticker.sell),
          bidPrice: new decimal(ticker.buy)));
    }
  }
}
