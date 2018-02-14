using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges.Exchanges.GDax
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// https://docs.gdax.com/#introduction
  /// </remarks>
  internal class GDaxExchange : Exchange
  {
    readonly IRestClient restClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exchangeMonitor"></param>
    public GDaxExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.GDax, 3 * 60)
    {
      restClient = new RestClient("https://api.gdax.com");
      exchangeMonitor.AddAlias("Ether", "Ethereum");
    }

    protected override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();

      List<GDaxTickerNameJson> productList =
        restClient.Get<List<GDaxTickerNameJson>>("currencies");

      for (int i = 0; i < productList.Count; i++)
      {
        GDaxTickerNameJson product = productList[i];
        bool isInactive = product.status.Equals("online",
          StringComparison.InvariantCultureIgnoreCase) == false;
        AddTicker(product.id, Coin.FromName(product.name), isInactive);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      await throttle.WaitTillReady();

      List< GDaxProductJson> productList =
            restClient.Get<List<GDaxProductJson>>("products");

      for (int i = 0; i < productList.Count; i++)
      {
        GDaxProductJson product = productList[i];
        bool isInactive = product.status.Equals("online", 
          StringComparison.InvariantCultureIgnoreCase) == false;

        await throttle.WaitTillReady();
        GDaxProductTickerJson productTicker
          = restClient.Get<GDaxProductTickerJson>($"products/{product.id}/ticker");

        AddTradingPair(product.quote_currency, product.base_currency,
          decimal.Parse(productTicker.ask),
          decimal.Parse(productTicker.bid),
          isInactive);
      }
    }
  }
}
