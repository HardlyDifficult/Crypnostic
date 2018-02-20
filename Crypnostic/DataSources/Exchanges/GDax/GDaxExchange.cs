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
    public GDaxExchange()
      : base(ExchangeName.GDax, "https://api.gdax.com", 3 * 60)
    {
      CrypnosticController.instance.AddCoinAlias("Ethereum", "Ether");
    }

    protected override async Task RefreshTickers()
    {
      List<GDaxTickerNameJson> productList =
        await Get<List<GDaxTickerNameJson>>("currencies");
      if(productList == null)
      {
        return;
      }

      for (int i = 0; i < productList.Count; i++)
      {
        GDaxTickerNameJson product = productList[i];
        bool isInactive = product.status.Equals("online",
          StringComparison.InvariantCultureIgnoreCase) == false;
        await AddTicker(await CreateFromName(product.name), product.id, isInactive);
      }
    }

    protected override async Task RefreshTradingPairs()
    {
      await throttle.WaitTillReady();

      List<GDaxProductJson> productList = await Get<List<GDaxProductJson>>("products");
      if(productList == null)
      {
        return;
      }

      for (int i = 0; i < productList.Count; i++)
      {
        GDaxProductJson product = productList[i];
        bool isInactive = product.status.Equals("online",
          StringComparison.InvariantCultureIgnoreCase) == false;

        GDaxProductTickerJson productTicker
          = await Get<GDaxProductTickerJson>($"products/{product.id}/ticker");
        if(productTicker == null)
        {
          continue;
        }

        TradingPair pair = await AddTradingPair(product.quote_currency, product.base_currency,
          decimal.Parse(productTicker.ask),
          decimal.Parse(productTicker.bid),
          isInactive);

        if (pair != null)
        {
          pair.lastTrade = new LastTrade(
            decimal.Parse(productTicker.price));
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol}-{baseSymbol}";
    }

    protected override async Task<OrderBook> GetOrderBook(
     string pairId)
    {
      GDaxDepthListJson depthJson = await Get<GDaxDepthListJson>($"products/{pairId}/book?level=2");
      if(depthJson == null)
      {
        return default(OrderBook);
      }

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
