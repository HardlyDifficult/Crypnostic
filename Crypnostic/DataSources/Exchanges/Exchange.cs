using HD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Common.Logging;
using Crypnostic.Internal;

namespace Crypnostic
{
  /// <summary>
  /// Represents a single exchange, e.g. Cryptopia.
  /// 
  /// You can find Exchanges with CrypnosticController.GetExchange().
  /// </summary>
  public abstract class Exchange
  {
    #region Public Data
    public readonly ExchangeName exchangeName;

    /// <summary>
    /// Called each time all prices on the exchange are updated.
    /// 
    /// This is an alternative to each Coin's onPriceUpdate event.
    /// </summary>
    public event Action<Exchange> onPriceUpdate;

    /// <summary>
    /// Called each time all statuses on the exchange are updated.
    /// 
    /// This is an alternative to each Coin's onStatusUpdate event 
    /// (which is only called if the coin's status change while this
    /// is called anytime we refresh the information).
    /// </summary>
    public event Action<Exchange> onStatusUpdate;

    /// <summary>
    /// Called each time a coin is detected on this exchange that
    /// that was not previously seen on this exchange.
    /// 
    /// Note: CrypnosticController has an onNewCoin event to detect brand new coins.
    /// </summary>
    public event Action<Exchange, Coin> onNewCoin;
    #endregion

    #region Internal Data
    internal protected readonly Dictionary<string, Coin>
      tickerLowerToCoin = new Dictionary<string, Coin>();

    internal protected readonly Dictionary<Coin, string>
      coinToTickerLower = new Dictionary<Coin, string>();

    protected readonly Throttle throttle;

    internal readonly HashSet<OrderBook> autoUpdatingBooks = new HashSet<OrderBook>();

    internal readonly HashSet<LastTrade> autoLastTrades = new HashSet<LastTrade>();

    readonly AutoUpdateWithThrottle autoUpdate;

    readonly HashSet<string> blacklistedTickerLower = new HashSet<string>();

    /// <summary>
    /// Tracks the Coin+Exchange status.
    /// </summary>
    readonly HashSet<Coin> inactiveCoins = new HashSet<Coin>();

    DateTime lastLoadTickerNames;

    readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

    static readonly ILog log = LogManager.GetLogger<Exchange>();
    #endregion

    #region Public Properties
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

    public IEnumerable<Coin> allCoins
    {
      get
      {
        return coinToTickerLower.Keys;
      }
    }
    #endregion

    #region Init
    internal static Exchange LoadExchange(
      ExchangeName exchangeName)
    {
      switch (exchangeName)
      {
        case ExchangeName.Binance:
          return new BinanceExchange();
        case ExchangeName.Cryptopia:
          return new CryptopiaExchange();
        //case ExchangeName.EtherDelta:
        //  return new EtherDeltaExchange();
        case ExchangeName.Kucoin:
          return new KucoinExchange();
        case ExchangeName.GDax:
          return new GDaxExchange();
        case ExchangeName.Idex:
          return new IdexExchange();
        case ExchangeName.AEX:
          return new AEXExchange();
        default:
          Debug.Fail("Missing Exchange");
          return null;
      }
    }

    /// <param name="timeBetweenAutoUpdates">Defaults to 10 seconds</param>
    protected Exchange(
      ExchangeName exchangeName,
      int maxRequestsPerMinute = 60,
      TimeSpan timeBetweenAutoUpdates = default(TimeSpan))
    {
      Debug.Assert(maxRequestsPerMinute > 0);

      if (timeBetweenAutoUpdates <= TimeSpan.Zero)
      {
        timeBetweenAutoUpdates = TimeSpan.FromSeconds(10);
      }

      this.exchangeName = exchangeName;
      this.throttle = new Throttle(TimeSpan.FromMinutes(2 * 1.0 / maxRequestsPerMinute),
        TimeSpan.FromMinutes(1));
      this.autoUpdate = new AutoUpdateWithThrottle(OnAutoUpdate,
        timeBetweenAutoUpdates,
        throttle,
        CrypnosticController.instance.cancellationTokenSource.Token);
    }

    internal async Task Start()
    {
      await autoUpdate.StartWithImmediateResults();
    }
    #endregion

    #region Events
    async Task OnAutoUpdate()
    {
      await RefreshAsync(TimeSpan.Zero);
    }
    #endregion

    #region Public Write API
    /// <summary>
    /// Call to prevent a ticker on this exchange from being considered.
    /// 
    /// Call this before calling CryptnosticController.Start().
    /// </summary>
    /// <param name="tickerList"></param>
    public void AddBlacklistedTicker(
      params string[] tickerList)
    {
      Debug.Assert(tickerList != null);
      Debug.Assert(tickerList.Length > 0);
      Debug.Assert(lastLoadTickerNames == default(DateTime));

      for (int i = 0; i < tickerList.Length; i++)
      {
        string ticker = tickerList[i];
        Debug.Assert(string.IsNullOrWhiteSpace(ticker) == false);

        ticker = ticker.ToLowerInvariant();
        blacklistedTickerLower.Add(ticker);
      }
    }

    /// <summary>
    /// Get the best bid and ask for each supported trading pair (coin+coin).
    /// May also get the list of coins on this exchange and their statuses.
    /// 
    /// This is called during init, after LoadTickerNames and then refreshed periodically.
    /// You can also call this anytime for a manual refresh (subject to throttling).
    /// </summary>
    /// <param name="timeBetweenTickersRefresh">
    /// Use the cache of supported tickers unless it's more than this old.
    /// </param>
    /// <remarks>
    /// It will call AddTicker for each coin.
    /// This may call UpdateTradingPair with status 
    /// (unless that is done during GetAllTradingPairs)
    /// 
    /// Call AddTradingPair for each pair supported.
    /// </remarks>
    public async Task RefreshAsync(
      TimeSpan timeBetweenTickersRefresh)
    {
      log.Info("Refresh begin for " + exchangeName);

      if (tickerLowerToCoin.Count == 0
        || DateTime.Now - lastLoadTickerNames > timeBetweenTickersRefresh)
      {
        await RefreshTickers();
        lastLoadTickerNames = DateTime.Now;
        onStatusUpdate?.Invoke(this);
      }

      await RefreshTradingPairs();

      // How-to do this better (events may change these lists anytime)
      OrderBook[] booksToUpdate = autoUpdatingBooks.ToArray();
      LastTrade[] tradesToUpdate = autoLastTrades.ToArray();

      Task[] taskList = new Task[(booksToUpdate?.Length ?? 0) + (tradesToUpdate?.Length ?? 0)];
      int i = 0;
      
      if (booksToUpdate != null)
      {
        foreach (OrderBook orderBook in booksToUpdate)
        {
          taskList[i++] = orderBook.RefreshAsync();
        }
      }
      if (tradesToUpdate != null)
      {
        foreach (LastTrade lastTrade in tradesToUpdate)
        {
          taskList[i++] = lastTrade.RefreshAsync();
        }
      }

      await Task.WhenAll(taskList);

      onPriceUpdate?.Invoke(this);

      log.Info("Refresh end for " + exchangeName);
    }
    #endregion

    #region Internal Write
    internal async Task UpdateOrderBook(
      OrderBook orderBook)
    {
      string pairId = GetPairId(orderBook.tradingPair.quoteCoin, orderBook.tradingPair.baseCoin);
      await UpdateOrderBook(pairId, orderBook);
    }

    protected abstract Task UpdateOrderBook(
      string pairId,
      OrderBook orderBook);

    protected abstract Task RefreshTickers();

    protected abstract Task RefreshTradingPairs();

    /// <summary>
    /// Override if the last trade is not already available 
    /// from an API which auto-refreshes.
    /// </summary>
    internal virtual Task RefreshLastTrade(
      TradingPair tradingPair)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// Register the ticker and status for a coin on this exchange.
    /// </summary>
    protected async Task AddTicker(
      Coin coin,
      string ticker,
      bool isInactive)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(ticker) == false);

      if (coin == null)
      { // Coin may be blacklisted
        return;
      }

      ticker = ticker.ToLowerInvariant();
      if (blacklistedTickerLower.Contains(ticker))
      { // Ticker blacklisted on this exchange
        return;
      }

      try
      {
        await semaphore.WaitAsync();

        if (isInactive)
        {
          inactiveCoins.Add(coin);
        }
        else
        {
          inactiveCoins.Remove(coin);
        }

        if (tickerLowerToCoin.ContainsKey(ticker))
        { // Ignore dupes
          Debug.Assert(tickerLowerToCoin[ticker] == coin);
          return;
        }

        if (coinToTickerLower.TryGetValue(coin, out string otherTicker))
        { // Dupe, find the more legit version by checking books
          tickerLowerToCoin.Remove(otherTicker);
          string preferredTicker = GetPreferredTicker(ticker, otherTicker, coin);
          preferredTicker = preferredTicker.ToLowerInvariant();
          coinToTickerLower[coin] = preferredTicker;
          tickerLowerToCoin.Add(preferredTicker, coin);
        }
        else
        {
          tickerLowerToCoin.Add(ticker, coin);
          coinToTickerLower.Add(coin, ticker);
        }

        onNewCoin?.Invoke(this, coin);
      }
      finally
      {
        semaphore.Release();
      }
    }

    protected virtual string GetPreferredTicker(
      string ticker,
      string otherTicker,
      Coin coin)
    {
      ticker = ticker.ToLowerInvariant();
      otherTicker.ToLowerInvariant();

      if (ticker.Contains("new")
        || otherTicker.Contains("old"))
      {
        return ticker;
      }
      if (ticker.Contains("old")
        || otherTicker.Contains("new"))
      {
        return otherTicker;
      }

      Debug.Fail(""); // Now what? exchange specific?
      return null;
    }

    /// <summary>
    /// Add or update a trading pair for this exchange.
    /// </summary>
    internal async Task<TradingPair> AddTradingPair(
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

      return await AddTradingPair(quoteCoinTicker, baseCoin, askPrice, bidPrice, isInactive);
    }

    /// <summary>
    /// Add or update a trading pair for this exchange.
    /// </summary>
    internal async Task<TradingPair> AddTradingPair(
      string quoteCoinTicker,
      Coin baseCoin,
      decimal askPriceOrOfferYouCanBuy,
      decimal bidPriceOrOfferYouCanSell,
      bool? isInactive)
    {
      Debug.Assert(baseCoin != null);
      Debug.Assert(string.IsNullOrWhiteSpace(quoteCoinTicker) == false);

      if (tickerLowerToCoin.TryGetValue(quoteCoinTicker.ToLowerInvariant(),
        out Coin quoteCoin) == false)
      { // May be missing due to book's listing status
        return null;
      }

      return await AddTradingPair(quoteCoin, baseCoin, askPriceOrOfferYouCanBuy, bidPriceOrOfferYouCanSell, isInactive);
    }

    internal async Task<TradingPair> AddTradingPair(
      Coin quoteCoin,
      Coin baseCoin,
      decimal askPriceOrOfferYouCanBuy,
      decimal bidPriceOrOfferYouCanSell,
      bool? isInactive)
    {
      try
      {
        await semaphore.WaitAsync();

        TradingPair pair = await quoteCoin.AddPair(this,
          baseCoin,
          askPriceOrOfferYouCanBuy,
          bidPriceOrOfferYouCanSell);

        if (isInactive != null)
        {
          pair.isInactive = isInactive.Value;
        }

        return pair;
      }
      finally
      {
        semaphore.Release();
      }
    }

    /// <summary>
    /// Get or create a coin if it is not blacklisted by this exchange.
    /// </summary>
    protected async Task<Coin> CreateFromName(
      string fullName)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);

      if (blacklistedTickerLower.Contains(fullName.ToLowerInvariant()))
      {
        return null;
      }

      return await CrypnosticController.instance.CreateFromName(fullName);
    }
    #endregion

    #region Public Read API
    /// <summary>
    /// False if the coin is not currently listed by this exchange
    /// or if the exchange is reporting an issue with the coin.
    /// </summary>
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

    #region Internal Read
    protected string GetPairId(
      Coin quoteCoin,
      Coin baseCoin)
    {
      if (coinToTickerLower.TryGetValue(quoteCoin, out string quoteCoinTicker) == false
        || coinToTickerLower.TryGetValue(baseCoin, out string baseCoinTicker) == false)
      {
        return null;
      }

      return GetPairId(quoteCoinTicker, baseCoinTicker);
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
