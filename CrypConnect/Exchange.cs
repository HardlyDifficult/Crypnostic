using CryptoExchanges.Exchanges.GDax;
using HD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace CryptoExchanges
{
  public abstract class Exchange
  {
    #region Public Data
    public readonly ExchangeName exchangeName;

    /// <summary>
    /// Called each time the exchange's prices are updated.
    /// This is an alternative to each Coin's onPriceUpdate event.
    /// </summary>
    public event Action<Exchange> onPriceUpdate;

    /// <summary>
    /// True if the exchange allows a negative spread.
    /// </summary>
    public virtual bool supportsOverlappingBooks
    {
      get
      {
        return false;
      }
    }
    #endregion

    #region Data
    protected readonly ExchangeMonitor exchangeMonitor;

    internal protected readonly Dictionary<string, Coin>
      tickerLowerToCoin = new Dictionary<string, Coin>();

    protected readonly Throttle throttle;

    readonly Timer timerRefreshData;

    readonly HashSet<Coin> inactiveCoinFullNames = new HashSet<Coin>();

    protected readonly HashSet<(Coin quoteCoin, Coin baseCoin)> inactivePairs
      = new HashSet<(Coin quoteCoin, Coin baseCoin)>();
    #endregion

    #region Init
    public static Exchange LoadExchange(
      ExchangeMonitor exchangeMonitor,
      ExchangeName exchangeName,
      bool includeMaintainceStatus)
    {
      switch (exchangeName)
      {
        case ExchangeName.Binance:
          return new BinanceExchange(exchangeMonitor);
        case ExchangeName.Cryptopia:
          return new CryptopiaExchange(
            exchangeMonitor,
            includeMaintainceStatus);
        case ExchangeName.EtherDelta:
          return new EtherDeltaExchange(exchangeMonitor);
        case ExchangeName.Kucoin:
          return new KucoinExchange(exchangeMonitor);
        case ExchangeName.GDax:
          return new GDaxExchange(exchangeMonitor);
        default:
          Debug.Fail("Missing Exchange");
          return null;
      }
    }

    protected Exchange(
      ExchangeMonitor exchangeMonitor,
      ExchangeName exchangeName,
      int maxRequestsPerMinute)
    {
      this.exchangeMonitor = exchangeMonitor;
      this.exchangeName = exchangeName;

      // Set the throttle to half the stated max requests per min
      throttle = new Throttle(TimeSpan.FromMilliseconds(
        2 * TimeSpan.FromMinutes(1).TotalMilliseconds / maxRequestsPerMinute));

      timerRefreshData = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
      timerRefreshData.AutoReset = false;
      timerRefreshData.Elapsed += Timer_Elapsed;
    }
    #endregion

    #region Public API
    public async Task GetAllPairs()
    {
      if (tickerLowerToCoin.Count == 0)
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
              await Task.Delay(TimeSpan.FromSeconds(20 + ExchangeMonitor.instance.random.Next(30)));
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
            await Task.Delay(TimeSpan.FromSeconds(20 + ExchangeMonitor.instance.random.Next(30)));
            continue;
          }
        }
        break;
      }

      onPriceUpdate?.Invoke(this);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ticker"></param>
    /// <param name="fullName"></param>
    /// <param name="isCoinActive">TODO for other exchanges</param>
    protected void AddTicker(
      string ticker,
      Coin coin,
      bool isCoinActive = true)
    {
      if (coin == null)
      { // Coin may be blacklisted
        return;
      }

      if (isCoinActive)
      {
        inactiveCoinFullNames.Remove(coin);
      }
      else
      {
        inactiveCoinFullNames.Add(coin);
      }

      ticker = ticker.ToLowerInvariant();

      if (tickerLowerToCoin.ContainsKey(ticker))
      { // Ignore dupes
        Debug.Assert(tickerLowerToCoin[ticker] == coin);
        return;
      }

      tickerLowerToCoin.Add(ticker, coin);
    }

    protected void AddTradingPairs<T>(
      IEnumerable<T> tickerList,
      Func<T,
        (string baseCoinTicker, string quoteCoinTicker, decimal askPrice, decimal bidPrice)> typeMapFunc)
    {
      if (tickerList == null)
      {
        return;
      }
      foreach (T ticker in tickerList)
      {
        (string baseCoinTicker, string quoteCoinTicker, decimal askPrice, decimal bidPrice)
            = typeMapFunc(ticker);
        AddTradingPair(baseCoinTicker, quoteCoinTicker, askPrice, bidPrice);
      }
    }

    public void AddTradingPair(
      string baseCoinTicker,
      string quoteCoinTicker,
      decimal askPrice,
      decimal bidPrice)
    {
      if (string.IsNullOrWhiteSpace(baseCoinTicker)
        || string.IsNullOrWhiteSpace(quoteCoinTicker))
      {
        return;
      }

      Debug.Assert(askPrice == 0 
        || bidPrice == 0 
        || askPrice >= bidPrice 
        || supportsOverlappingBooks);

      if (tickerLowerToCoin.TryGetValue(baseCoinTicker.ToLowerInvariant(),
        out Coin baseCoin) == false)
      { // May be missing due to coin filtering (e.g. no Tether)
        return;
      }
      if (tickerLowerToCoin.TryGetValue(quoteCoinTicker.ToLowerInvariant(),
        out Coin quoteCoin) == false)
      { // May be missing due to book's listing status
        return;
      }
      if (inactiveCoinFullNames.Contains(baseCoin)
        || inactiveCoinFullNames.Contains(quoteCoin))
      {
        return;
      }
      if (inactivePairs.Contains((quoteCoin, baseCoin)))
      {
        return;
      }

      new TradingPair(
        this,
        baseCoin,
        quoteCoin,
        askPrice,
        bidPrice);
    }
    #endregion
  }
}
