using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  public abstract class Exchange
  {
    #region Data
    protected readonly ExchangeMonitor exchangeMonitor;

    public readonly ExchangeName exchangeName;

    protected readonly Dictionary<string, string>
      tickerToFullName = new Dictionary<string, string>();
    #endregion

    #region Init
    public static Exchange LoadExchange(
      ExchangeMonitor exchangeMonitor,
      ExchangeName exchangeName)
    {
      switch (exchangeName)
      {
        case ExchangeName.Binance:
          return new BinanceExchange(exchangeMonitor);
        case ExchangeName.Cryptopia:
          return new CryptopiaExchange(exchangeMonitor);
        case ExchangeName.EtherDelta:
          return new EtherDeltaExchange(exchangeMonitor);
        case ExchangeName.Kucoin:
          return new KucoinExchange(exchangeMonitor);
        default:
          Debug.Fail("Missing Exchange");
          return null;
      }
    }

    protected Exchange(
      ExchangeMonitor exchangeMonitor,
      ExchangeName exchangeName)
    {
      this.exchangeMonitor = exchangeMonitor;
      this.exchangeName = exchangeName;
    }
    #endregion

    #region Public API
    public decimal? GetConversion(
      string fromOrQuoteCoinFullName, // ETH or BTC
      string toOrBaseCoinFullName, // BTC or BNB
      bool sellVsBuy)
    {
      // TODO prefer this exchange if we can
      Coin fromCoin = exchangeMonitor.FindCoin(fromOrQuoteCoinFullName);

      TradingPair pair = fromCoin?.Best(sellVsBuy, toOrBaseCoinFullName, true);
      if (pair == null)
      {
        fromCoin = exchangeMonitor.FindCoin(toOrBaseCoinFullName);
        pair = fromCoin?.Best(sellVsBuy, fromOrQuoteCoinFullName);
        if (pair == null)
        {
          return null;
        }
      }

      return sellVsBuy ? pair.bidPrice : pair.askPrice;
    }

    public async Task GetAllPairs()
    {
      if (tickerToFullName.Count == 0)
      {
        LoadTickerNames();
      }

      await GetAllTradingPairs();
    }
    #endregion

    #region Helpers
    protected abstract void LoadTickerNames();

    protected abstract Task GetAllTradingPairs();

    protected void AddTicker(
      string ticker,
      string fullName)
    {
      if (tickerToFullName.ContainsKey(ticker))
      { // Ignore dupes
        return;
      }
      tickerToFullName.Add(ticker, fullName);
    }

    protected void AddTradingPairs<T>(
      IEnumerable<T> tickerList,
      Func<T,
        (string baseCoin, string quoteCoin, decimal askPrice, decimal bidPrice)> typeMapFunc)
    {
      if (tickerList == null)
      {
        return;
      }

      foreach (T ticker in tickerList)
      {
        (string baseCoin, string quoteCoin, decimal askPrice, decimal bidPrice) = typeMapFunc(ticker);
        if (baseCoin == null || quoteCoin == null)
        {
          continue;
        }
        Console.WriteLine(quoteCoin);
        if (tickerToFullName.TryGetValue(baseCoin, out string baseCoinFullName) == false)
        {
          Console.WriteLine($"Missing base: {baseCoin}"); // TODO
          continue;
        }
        if (tickerToFullName.TryGetValue(quoteCoin, out string quoteCoinFullName) == false)
        {
          Console.WriteLine("Missing");// TODO
          continue;
        }

        TradingPair pair = new TradingPair(
          this,
          baseCoinFullName,
          quoteCoinFullName,
          askPrice,
          bidPrice);

        exchangeMonitor.AddPair(pair);
      }
    }
    #endregion
  }
}
