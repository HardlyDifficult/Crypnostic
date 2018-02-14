using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CrypConnect.Exchanges.GDax
{
  /// <remarks>
  /// https://docs.gdax.com/#introduction
  /// </remarks>
  internal class GDaxExchange : RestExchange
  {
    public GDaxExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.GDax, 3 * 60, "https://api.gdax.com")
    {
      exchangeMonitor.AddAlias("Ether", "Ethereum");
    }

    public override async Task LoadTickerNames()
    {
      List<GDaxTickerNameJson> productList =
        await Get<List<GDaxTickerNameJson>>("currencies");

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

      List<GDaxProductJson> productList = await Get<List<GDaxProductJson>>("products");

      for (int i = 0; i < productList.Count; i++)
      {
        GDaxProductJson product = productList[i];
        bool isInactive = product.status.Equals("online", 
          StringComparison.InvariantCultureIgnoreCase) == false;

        GDaxProductTickerJson productTicker
          = await Get<GDaxProductTickerJson>($"products/{product.id}/ticker");

        AddTradingPair(product.quote_currency, product.base_currency,
          decimal.Parse(productTicker.ask),
          decimal.Parse(productTicker.bid),
          isInactive);
      }
    }
  }
}
