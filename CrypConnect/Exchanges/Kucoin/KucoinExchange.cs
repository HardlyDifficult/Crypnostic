using System;
using System.Threading.Tasks;
using CrypConnect.Exchanges.Kucoin;

namespace CrypConnect.Exchanges
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
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Kucoin, 1_000_000 / 1_440, "https://api.kucoin.com")
    {
      exchangeMonitor.AddAlias("USDT", "Tether");
      exchangeMonitor.AddAlias("Raiden Network", "Raiden Network Token");
      exchangeMonitor.AddAlias("Request", "Request Network");
      exchangeMonitor.AddAlias("TenXPay", "TenX");
    }

    public override async Task LoadTickerNames()
    {
      KucoinProductListJson productList = await Get<KucoinProductListJson>(
        "v1/market/open/coins");
      foreach (KucoinProductListJson.ProductJson product in productList.data)
      {
        string ticker = product.coin;
        string fullName = product.name;
        Coin coin = Coin.CreateFromName(fullName);
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
  }
}
