using System;
using System.Threading.Tasks;

namespace Crypnostic
{
  /// <remarks>
  /// https://kucoinapidocs.docs.apiary.io/#
  /// </remarks>
  internal class KucoinExchange : RestExchange
  {
    public override bool supportsOverlappingBooks
    {
      get
      { // Rare, but it happens 
        return true;
      }
    }

    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    public KucoinExchange()
      : base(ExchangeName.Kucoin, "https://api.kucoin.com", 1_000_000 / 1_440)
    {
      CrypnosticController.instance.AddCoinAlias(
        new[] { "Tether", "USDT" },
        new[] { "Raiden Network Token", "Raiden Network" },
        new[] { "Request Network", "Request" },
        new[] { "TenX", "TenXPay" },
        new[] { "CanYaCoin", "CanYa" },
        new[] { "BlockMason Credit Protocol", "BlockMason" },
        new[] { "High Performance Blockchain", "High Performance Blockch" },
        new[] { "Gas", "NeoGas" },
        new[] { "Oyster", "Oyster Pearl" },
        new[] { "Trinity Network Credit", "Trinity" });
    }

    protected override async Task RefreshTickers()
    {
      KucoinProductListJson productList = await Get<KucoinProductListJson>(
        "v1/market/open/coins");
      if(productList == null)
      {
        return;
      }

      foreach (KucoinProductListJson.ProductJson product in productList.data)
      {
        string ticker = product.coin;
        string fullName = product.name;
        Coin coin = await CreateFromName(fullName);
        bool isInactive = product.enableDeposit == false || product.enableWithdraw == false;
        await AddTicker(coin, ticker, isInactive);
      }
    }

    protected override async Task RefreshTradingPairs()
    {
      KucoinTickerListJson tickerList = await Get<KucoinTickerListJson>("v1/open/tick");
      if(tickerList == null)
      {
        return;
      }

      foreach (KucoinTickerListJson.TickerJson ticker in tickerList.data)
      {
        string baseCoinTicker = ticker.coinTypePair;
        string quoteCoinTicker = ticker.coinType;
        decimal askPrice = new decimal(ticker.sell ?? 0);
        decimal bidPrice = new decimal(ticker.buy ?? 0);
        bool isInactive = ticker.trading == false;

        TradingPair pair = await AddTradingPair(baseCoinTicker, quoteCoinTicker, askPrice, bidPrice, isInactive);

        if (pair != null)
        {
          pair.lastTrade = new LastTrade(
            new decimal(ticker.lastDealPrice));
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}-{baseSymbol.ToUpperInvariant()}";
    }

    protected override async Task<OrderBook> GetOrderBook(
     string pairId)
    {
      KucoinDepthListJson depthJson = await Get<KucoinDepthListJson>(
        $"v1/open/orders?symbol={pairId}&limit=100");
      if(depthJson == null)
      {
        return default(OrderBook);
      }

      Order[] bids = ExtractOrders(depthJson.data.BUY);
      Order[] asks = ExtractOrders(depthJson.data.SELL);

      return new OrderBook(asks, bids);
    }

    static Order[] ExtractOrders(
      decimal[][] resultList)
    {
      if (resultList == null)
      {
        return new Order[0];
      }

      Order[] orderList = new Order[resultList.Length];
      for (int i = 0; i < orderList.Length; i++)
      {
        orderList[i] = new Order(resultList[i][0], resultList[i][1]);
      }

      return orderList;
    }
  }
}
