using System;
using System.Threading.Tasks;
using Crypnostic.Exchanges.Kucoin;

namespace Crypnostic.Exchanges
{
  /// <remarks>
  /// https://kucoinapidocs.docs.apiary.io/#
  /// </remarks>
  internal class KucoinExchange : RestExchange
  {
    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    public KucoinExchange(
      CrypnosticController exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Kucoin, 1_000_000 / 1_440, "https://api.kucoin.com")
    {
      exchangeMonitor.AddAlias("USDT", "Tether");
      exchangeMonitor.AddAlias("Raiden Network", "Raiden Network Token");
      exchangeMonitor.AddAlias("Request", "Request Network");
      exchangeMonitor.AddAlias("TenXPay", "TenX");
      exchangeMonitor.AddAlias("CanYa", "CanYaCoin");
      exchangeMonitor.AddAlias("BlockMason", "BlockMason Credit Protocol");
      exchangeMonitor.AddAlias("High Performance Blockch", "High Performance Blockchain");
      exchangeMonitor.AddAlias("NeoGas", "Gas");
      exchangeMonitor.AddAlias("Oyster Pearl", "Oyster");
      exchangeMonitor.AddAlias("Trinity", "Trinity Network Credit");
    }

    public override async Task LoadTickerNames()
    {
      KucoinProductListJson productList = await Get<KucoinProductListJson>(
        "v1/market/open/coins");
      foreach (KucoinProductListJson.ProductJson product in productList.data)
      {
        string ticker = product.coin;
        string fullName = product.name;
        Coin coin = CreateFromName(fullName);
        bool isInactive = product.enableDeposit == false || product.enableWithdraw == false;
        AddTicker(ticker, coin, isInactive);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      KucoinTickerListJson tickerList = await Get<KucoinTickerListJson>("v1/open/tick");

      foreach (KucoinTickerListJson.TickerJson ticker in tickerList.data)
      {
        string baseCoinTicker = ticker.coinTypePair;
        string quoteCoinTicker = ticker.coinType;
        decimal askPrice = new decimal(ticker.sell ?? 0);
        decimal bidPrice = new decimal(ticker.buy ?? 0);
        bool isInactive = ticker.trading == false;

        TradingPair pair = AddTradingPair(baseCoinTicker, quoteCoinTicker, askPrice, bidPrice, isInactive);

        if (pair != null)
        {
          pair.lastTrade = new LastTrade(
            new decimal(ticker.lastDealPrice), new decimal(ticker.vol));
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}-{baseSymbol.ToUpperInvariant()}";
    }

    protected override async Task<OrderBook> GetOrderBookInternal(
     string pairId)
    {
      KucoinDepthListJson depthJson = await Get<KucoinDepthListJson>(
        $"v1/open/orders?symbol={pairId}&limit=100");

      Order[] bids = ExtractOrders(depthJson.data.BUY);
      Order[] asks = ExtractOrders(depthJson.data.SELL);

      return new OrderBook(asks, bids);
    }

    static Order[] ExtractOrders(
      decimal[][] resultList)
    {
      if(resultList == null)
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
