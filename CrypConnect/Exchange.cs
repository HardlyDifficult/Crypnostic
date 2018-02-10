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
    public readonly ExchangeName exchangeName;

    protected readonly Dictionary<string, string>
      tickerToFullName = new Dictionary<string, string>();

    protected static readonly Random random = new Random();
    #endregion

    #region Init
    public static Exchange LoadExchange(
      ExchangeName exchangeName)
    {
      switch (exchangeName)
      {
        case ExchangeName.Binance:
          return new BinanceExchange();
        case ExchangeName.Cryptopia:
          return new CryptopiaExchange();
        case ExchangeName.EtherDelta:
          return new EtherDeltaExchange();
        case ExchangeName.Kucoin:
          return new KucoinExchange();
        default:
          Debug.Fail("Missing Exchange");
          return null;
      }
    }

    protected Exchange(
      ExchangeName exchangeName)
    {
      this.exchangeName = exchangeName;
    }
    #endregion

    #region Public API
    public async Task<List<TradingPair>> GetAllPairs()
    {
      if (tickerToFullName.Count == 0)
      {
        LoadTickerNames();
      }

      return await GetAllTradingPairs();
    }
    #endregion

    #region Helpers
    protected abstract void LoadTickerNames();

    protected abstract Task<List<TradingPair>> GetAllTradingPairs();

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

    protected List<TradingPair> AddTradingPairs<T>(
      IEnumerable<T> tickerList,
      Func<T,
        (string baseCoin, string quoteCoin, decimal askPrice, decimal bidPrice)> typeMapFunc)
    {
      if (tickerList == null)
      {
        return null;
      }
      List<TradingPair> tradingPairList = new List<TradingPair>();

      foreach (T ticker in tickerList)
      {
        (string baseCoin, string quoteCoin, decimal askPrice, decimal bidPrice) = typeMapFunc(ticker);
        if (tickerToFullName.TryGetValue(baseCoin, out string baseCoinFullName) == false)
        {
          continue;
        }
        if (tickerToFullName.TryGetValue(quoteCoin, out string quoteCoinFullName) == false)
        {
          continue;
        }

        TradingPair pair = new TradingPair(
          exchangeName,
          baseCoinFullName,
          quoteCoinFullName,
          askPrice,
          bidPrice);
        tradingPairList.Add(pair);
      }

      return tradingPairList;
    }
    #endregion
  }
}
