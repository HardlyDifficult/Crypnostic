using HD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CryptoExchanges
{
  public abstract class Exchange
  {
    #region Data
    protected readonly ExchangeMonitor exchangeMonitor;

    public readonly ExchangeName exchangeName;

    protected readonly Dictionary<string, string>
      tickerToFullName = new Dictionary<string, string>();

    protected readonly Throttle throttle;

    readonly Timer timerRefreshData;
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
          return new CryptopiaExchange(
            exchangeMonitor,
            includeMaintainceStatus: false); // TODO an option for this?
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
      ExchangeName exchangeName,
      TimeSpan timeBetweenRequests)
    {
      this.exchangeMonitor = exchangeMonitor;
      this.exchangeName = exchangeName;

      throttle = new Throttle(timeBetweenRequests);

      timerRefreshData = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
      timerRefreshData.AutoReset = false;
      timerRefreshData.Elapsed += Timer_Elapsed;
    }
    #endregion

    #region Public API
    public decimal? GetConversion(
      string fromOrQuoteCoinFullName,
      string toOrBaseCoinFullName,
      bool sellVsBuy)
    {
      // TODO prefer this exchange if we can
      Coin fromCoin = exchangeMonitor.FindCoin(fromOrQuoteCoinFullName);

      // TODO types
      (TradingPair pair, decimal todo) = fromCoin?.Best(sellVsBuy, toOrBaseCoinFullName, true) ?? (null, 0);
      if (pair == null)
      {
        fromCoin = exchangeMonitor.FindCoin(toOrBaseCoinFullName);
        (pair, todo) = fromCoin?.Best(sellVsBuy, fromOrQuoteCoinFullName) ?? (null, 0);
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
        while (true)
        {
          try
          {
            await LoadTickerNames();
          }
          catch
          { // Auto retry on fail
            if (exchangeMonitor.shouldStop == false)
            {
              await Task.Delay(TimeSpan.FromSeconds(20 + ExchangeMonitor.random.Next(30)));
              continue;
            }
          }
          break;
        }
      }

      await GetAllTradingPairsWrapper();
    }

    async Task GetAllTradingPairsWrapper()
    {
      while (true)
      {
        try
        {
          await GetAllTradingPairs();
        }
        catch
        { // Auto retry on fail
          if (exchangeMonitor.shouldStop == false)
          {
            await Task.Delay(TimeSpan.FromSeconds(20 + ExchangeMonitor.random.Next(30)));
            continue;
          }
        }
        break;
      }
      timerRefreshData.Start();
    }

    async void Timer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      await GetAllTradingPairsWrapper();
    }
    #endregion

    #region Helpers
    protected abstract Task LoadTickerNames();

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
        if (tickerToFullName.TryGetValue(baseCoin, out string baseCoinFullName) == false)
        { // May be missing due to coin filtering (e.g. no Tether)
          continue;
        }
        if (tickerToFullName.TryGetValue(quoteCoin, out string quoteCoinFullName) == false)
        { // May be missing due to book's listing status
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
