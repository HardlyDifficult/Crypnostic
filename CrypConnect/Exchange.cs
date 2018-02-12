using HD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace CryptoExchanges
{
  /// <summary>
  /// TODO
  ///  - Listing status for all but Cryptopia
  ///  - Periodically refresh the listing status
  /// </summary>
  public abstract class Exchange
  {
    #region Public Data
    public readonly ExchangeName exchangeName;
    #endregion

    #region Data
    protected readonly ExchangeMonitor exchangeMonitor;

    internal protected readonly Dictionary<string, Coin>
      tickerLowerToCoin = new Dictionary<string, Coin>();

    protected readonly Throttle throttle;

    readonly Timer timerRefreshData;

    // TODO use this
    readonly HashSet<Coin> inactiveCoinFullNames = new HashSet<Coin>();

    // TODO use this
    protected readonly HashSet<(Coin quoteCoin, Coin baseCoin)> inactivePairs
      = new HashSet<(Coin quoteCoin, Coin baseCoin)>();
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
    public decimal? GetConversion(
      Coin quoteCoin,
      Coin baseCoin,
      bool sellVsBuy)
    {
      Debug.Assert(quoteCoin != null);
      Debug.Assert(baseCoin != null);

      // TODO prefer this exchange if we can
      TradingPair pair = quoteCoin.Best(sellVsBuy, baseCoin, true);
      if (pair == null)
      {
        // TODO what's going on here? Something seems wrong.
        pair = baseCoin.Best(sellVsBuy, quoteCoin);
        if (pair == null)
        {
          return null;
        }
      }

      return sellVsBuy ? pair.bidPrice : pair.askPrice;
    }

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
          catch (Exception e)
          { // Auto retry on fail
            Console.WriteLine(e.ToString()); // TODO

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

      if (tickerLowerToCoin.ContainsKey(ticker))
      { // Ignore dupes
        Debug.Assert(tickerLowerToCoin[ticker] == coin);
        return;
      }

      tickerLowerToCoin.Add(ticker.ToLowerInvariant(), coin);
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

        if (string.IsNullOrWhiteSpace(baseCoinTicker)
          || string.IsNullOrWhiteSpace(quoteCoinTicker))
        {
          continue;
        }

        if (tickerLowerToCoin.TryGetValue(baseCoinTicker.ToLowerInvariant(),
          out Coin baseCoin) == false)
        { // May be missing due to coin filtering (e.g. no Tether)
          continue;
        }
        if (tickerLowerToCoin.TryGetValue(quoteCoinTicker.ToLowerInvariant(),
          out Coin quoteCoin) == false)
        { // May be missing due to book's listing status
          continue;
        }

        TradingPair pair = new TradingPair(
          this,
          baseCoin,
          quoteCoin,
          askPrice,
          bidPrice);

        // TODO
        pair.quoteCoin.AddPair(pair);
      }
    }
    #endregion
  }
}
