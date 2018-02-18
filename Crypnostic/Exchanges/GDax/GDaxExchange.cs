using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace Crypnostic.Exchanges.GDax
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
        AddTicker(product.id, CreateFromName(product.name), isInactive);
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

        TradingPair pair = AddTradingPair(product.quote_currency, product.base_currency,
          decimal.Parse(productTicker.ask),
          decimal.Parse(productTicker.bid),
          isInactive);

        if (pair != null)
        {
          pair.lastTrade = new LastTrade(
            decimal.Parse(productTicker.price),
            decimal.Parse(productTicker.size));
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol}-{baseSymbol}";
    }

    protected override async Task<OrderBook> GetOrderBookInternal(
     string pairId)
    {
      GDaxDepthListJson depthJson = await Get<GDaxDepthListJson>($"products/{pairId}/book?level=2");

      Order[] bids = ExtractOrders(depthJson.bids);
      Order[] asks = ExtractOrders(depthJson.asks);

      return new OrderBook(asks, bids);
    }

    static Order[] ExtractOrders(
      string[][] resultList)
    {
      Order[] orderList = new Order[resultList.Length];
      for (int i = 0; i < orderList.Length; i++)
      {
        orderList[i] = new Order(decimal.Parse(resultList[i][0]),
          decimal.Parse(resultList[i][1]));
      }

      return orderList;
    }
  }
}
