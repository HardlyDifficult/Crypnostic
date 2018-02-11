using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using CryptoExchanges.Kucoin;
using CryptoExchanges.Exchanges.Kucoin;

namespace CryptoExchanges
{
  internal class KucoinExchange : Exchange
  {
    readonly IRestClient restClient;

    public KucoinExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Kucoin)
    {
      restClient = new RestClient("https://api.kucoin.com");
    }

    protected override void LoadTickerNames()
    {
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
