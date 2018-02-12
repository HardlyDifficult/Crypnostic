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
      : base(exchangeMonitor, ExchangeName.Kucoin, 1_000_000 / 1_440)
    {
      restClient = new RestClient("https://api.kucoin.com");
    }

    protected override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();

      KucoinTickerNameListJson productList =
        restClient.Get<KucoinTickerNameListJson>("v1/market/open/coins");

      for (int i = 0; i < productList.data.Length; i++)
      {
        KucoinTickerNameJson product = productList.data[i];
        AddTicker(product.coin, Coin.FromName(product.name));
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      await throttle.WaitTillReady();

      KucoinMarketInfo tickerList =
            restClient.Get<KucoinMarketInfo>("v1/open/tick");
      AddTradingPairs(tickerList.data, (KucoinTradingPairJson ticker) =>
        (baseCoinTicker: ticker.coinTypePair,
          quoteCoinTicker: ticker.coinType,
          askPrice: new decimal(ticker.sell),
          bidPrice: new decimal(ticker.buy)));
    }
  }
}
