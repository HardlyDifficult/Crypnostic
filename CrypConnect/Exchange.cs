using CrypConnect.Exchanges;
using CrypConnect.Exchanges.GDax;
using HD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace CrypConnect
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

    internal virtual Task RefreshLastTrade(
      TradingPair tradingPair)
    {
      return null;
    }
    #endregion

    #region Data
    protected readonly ExchangeMonitor exchangeMonitor;

    internal protected readonly Dictionary<string, Coin>
      tickerLowerToCoin = new Dictionary<string, Coin>();

    internal protected readonly Dictionary<Coin, string>
      coinToTickerLower = new Dictionary<Coin, string>();

    protected readonly Throttle throttle;

    readonly Timer timerRefreshData;

    readonly HashSet<string> blacklistedTickerLower = new HashSet<string>();

    readonly HashSet<Coin> inactiveCoins = new HashSet<Coin>();

    readonly TimeSpan timeBetweenGetAllPairs = TimeSpan.FromMinutes(5); // TODO config

    DateTime lastLoadTickerNames;
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
        case ExchangeName.GDax:
          return new GDaxExchange(exchangeMonitor);
        case ExchangeName.Idex:
          return new IdexExchange(exchangeMonitor);
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

      timerRefreshData = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
      timerRefreshData.AutoReset = false;
      timerRefreshData.Elapsed += Timer_Elapsed;
    }
    #endregion

    #region Public API
    public void AddBlacklistedTicker(
      params string[] tickerList)
    {
      for (int i = 0; i < tickerList.Length; i++)
      {
        string ticker = tickerList[i];
        ticker = ticker.ToLowerInvariant();
        blacklistedTickerLower.Add(ticker);
      }
    }

    public async Task GetAllPairs(
      bool preventRetryOnFail = false)
    {
      if (tickerLowerToCoin.Count == 0 || DateTime.Now - lastLoadTickerNames > timeBetweenGetAllPairs)
      {
        do
        {
          try
          {
            await LoadTickerNames();
            lastLoadTickerNames = DateTime.Now;
          }
          catch (Exception e)
          { // Auto retry on fail
            Console.WriteLine(e); // TODO log
            if (exchangeMonitor.shouldStop == false)
            {
              await Task.Delay(TimeSpan.FromSeconds(20 + ExchangeMonitor.instance.random.Next(30)));
              continue;
            }
          }
          break;
        } while (preventRetryOnFail == false);
      }

      await GetAllTradingPairsWrapper();
    }

    async Task GetAllTradingPairsWrapper(
      bool preventRetryOnFail = false)
    {
      do
      {
        try
        {
          await GetAllTradingPairs();
        }
        catch (Exception e)
        { // Auto retry on fail
          // TODO log
          Console.WriteLine(e);

          if (exchangeMonitor.shouldStop == false)

          {
            await Task.Delay(TimeSpan.FromSeconds(20 + ExchangeMonitor.instance.random.Next(30)));
            continue;
          }
        }
        onPriceUpdate?.Invoke(this);
        break;
      } while (preventRetryOnFail == false);

      timerRefreshData.Start();
    }

    async void Timer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      await GetAllTradingPairsWrapper();
    }

    public bool IsCoinActive(
      Coin coin)
    {
      if (inactiveCoins.Contains(coin))
      {
        return false;
      }

      return tickerLowerToCoin.ContainsValue(coin);
    }
    #endregion

    #region Helpers

    /// <summary>
    /// This is called during init and then refreshed periodically.
    /// You can also call this anytime for a manual refresh (subject to throttling).
    /// It should call AddTicker for each coin.
    /// This may call UpdateTradingPair with status (unless that is done during GetAllTradingPairs)
    /// </summary>
    public abstract Task LoadTickerNames();

    /// <summary>
    /// This is called during init, after LoadTickerNames and then refreshed periodically.
    /// Call AddTradingPair for each pair supported.
    /// </summary>
    protected abstract Task GetAllTradingPairs();

    // TODO
    ///     ///  - Event when the coin status changes
    ///  - Event when a new coin is detected
    ///  TODO for other exchanges
    protected void AddTicker(
      string ticker,
      Coin coin,
      bool isCoinActive)
    {
      if (coin == null)
      { // Coin may be blacklisted
        return;
      }

      ticker = ticker.ToLowerInvariant();

      if (blacklistedTickerLower.Contains(ticker))
      { // Ticker blacklisted on this exchange
        return;
      }

      if (isCoinActive)
      {
        inactiveCoins.Remove(coin);
      }
      else
      {
        inactiveCoins.Add(coin);
      }


      if (tickerLowerToCoin.ContainsKey(ticker))
      { // Ignore dupes
        Debug.Assert(tickerLowerToCoin[ticker] == coin);
        return;
      }

      try
      {

        tickerLowerToCoin.Add(ticker, coin);
        coinToTickerLower.Add(coin, ticker);
      }
      catch
      { // TODO remove
        Console.WriteLine();
      }
    }

    public TradingPair AddTradingPair(
      string baseCoinTicker,
      string quoteCoinTicker,
      decimal askPrice,
      decimal bidPrice,
      bool? isInactive = null)
    {
      if (string.IsNullOrWhiteSpace(baseCoinTicker)
        || string.IsNullOrWhiteSpace(quoteCoinTicker))
      {
        return null;
      }

      Debug.Assert(askPrice == 0
        || bidPrice == 0
        || askPrice >= bidPrice
        || supportsOverlappingBooks);

      if (tickerLowerToCoin.TryGetValue(baseCoinTicker.ToLowerInvariant(),
        out Coin baseCoin) == false)
      { // May be missing due to coin filtering (e.g. no Tether)
        return null;
      }
      if (tickerLowerToCoin.TryGetValue(quoteCoinTicker.ToLowerInvariant(),
        out Coin quoteCoin) == false)
      { // May be missing due to book's listing status
        return null;
      }

      TradingPair pair = quoteCoin.AddPair(this,
        baseCoin,
        askPrice,
        bidPrice);

      if (isInactive != null)
      {
        pair.isInactive = isInactive.Value;
      }

      return pair;
    }

    protected string GetPairId(
      TradingPair pair)
    {
      string quoteSymbol = coinToTickerLower[pair.quoteCoin];
      string baseSymbol = coinToTickerLower[pair.baseCoin];

      return GetPairId(quoteSymbol, baseSymbol);
    }

    protected abstract string GetPairId(
      string quoteSymbol,
      string baseSymbol);
    #endregion
  }
}
