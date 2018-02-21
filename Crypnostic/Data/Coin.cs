using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HD;

namespace Crypnostic
{
  /// <summary>
  /// This tracks a single currency across all exchanges.  
  /// e.g. Ethereum.
  /// </summary>
  public class Coin
  {
    #region Public Static 
    /// <summary>
    /// Reference to Bitcoin, for convenience.
    /// </summary>
    public static Coin bitcoin
    {
      get
      {
        return FromName("Bitcoin");
      }
    }

    /// <summary>
    /// Reference to Ethereum, for convenience.
    /// </summary>
    public static Coin ethereum
    {
      get
      {
        return FromName("Ethereum");
      }
    }

    /// <summary>
    /// Reference to United States Dollar, for convenience.
    /// </summary>
    public static Coin usd
    {
      get
      {
        return FromName("United States Dollar");
      }
    }

    /// <summary>
    /// Reference to Litecoin, for convenience.
    /// </summary>
    public static Coin litecoin
    {
      get
      {
        return FromName("Litecoin");
      }
    }
    #endregion

    #region Public Data
    public readonly string fullName;

    public delegate void OnUpdate(Coin coin, TradingPair tradingPair);

    /// <summary>
    /// Called when prices refresh on any TradingPair for this Coin.
    /// </summary>
    public event OnUpdate onPriceUpdate;

    /// <summary>
    /// Called when a TradingPair for this Coin has a status change.
    /// e.g. switching from maintaince to running again
    /// </summary>
    public event OnUpdate onStatusUpdate;

    /// <summary>
    /// Called anytime a new exchange pairing is listed.
    /// 
    /// onPriceUpdated is also called anytime this event occurs.
    /// </summary>
    public event OnUpdate onNewTradingPairListed;

    /// <summary>
    /// Data about the coin from CoinMarketCap, if available.
    /// 
    /// The app will attempt to load this data on Start,
    /// and will then refresh periodically.
    /// </summary>
    public MarketCap coinMarketCapData;
    #endregion

    #region Private Data
    readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

    static readonly ILog log = LogManager.GetLogger<Coin>();

    /// <summary>
    /// Cached in this form for performance.
    /// </summary>
    internal readonly string fullNameLower;

    /// <summary>
    /// All known pairs for this coin.
    /// 
    /// TradingPairs are updated (not replaced) periodically.
    /// </summary>
    readonly Dictionary<(ExchangeName, Coin baseCoin), TradingPair> tradingPairs
      = new Dictionary<(ExchangeName, Coin baseCoin), TradingPair>();
    #endregion

    #region Public Properties
    /// <summary>
    /// Checks if there is both a valid bid and a valid ask
    /// for this coin.
    /// 
    /// Valid means:
    ///  - From a supported exchange
    ///  - Price > 0
    ///  - The exchange is not reporting an issue
    /// </summary>
    public bool hasValidTradingPairs
    {
      get
      {
        bool hasBid = false;
        bool hasAsk = false;
        foreach (var pair in tradingPairs)
        {
          if (pair.Value.isInactive)
          {
            continue;
          }
          if (pair.Value.bidPriceOrOfferYouCanSell > 0)
          {
            hasBid = true;
          }
          if (pair.Value.askPriceOrOfferYouCanBuy > 0)
          {
            hasAsk = true;
          }
          if (hasBid && hasAsk)
          {
            return true;
          }
        }

        return false;
      }
    }

    /// <summary>
    /// You can use this to review all known pairs for this coin.
    /// </summary>
    public IEnumerable<TradingPair> allTradingPairs
    {
      get
      {
        return tradingPairs.Values;
      }
    }
    #endregion

    #region Init
    /// <summary>
    /// Finds a coin by its full name.
    /// 
    /// When in doubt, use the name from CoinMarketCap.
    /// </summary>
    public static Coin FromName(
      string fullName)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);

      Task<Coin> task = CrypnosticController.instance.CreateFromName(fullName);
      task.Wait();

      return task.Result;
    }

    /// <summary>
    /// Finds a coin by its ticker.
    /// 
    /// Note that tickers are not standard:
    ///  - The same ticker may be different coins on different exchanges
    ///  - Different tickers may actually be the same coin.
    /// </summary>
    /// <param name="onExchange">
    /// If null, use ticker names from CoinMarketCap.
    /// </param>
    public static Coin FromTicker(
      string ticker,
      ExchangeName? onExchange)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(ticker) == false);

      ticker = ticker.ToLowerInvariant();

      Dictionary<string, Coin> tickerLowerToCoin;
      if (onExchange == null)
      {
        tickerLowerToCoin = CrypnosticController.instance.coinMarketCap.tickerLowerToCoin;
      }
      else
      {
        Exchange exchange = CrypnosticController.instance.GetExchange(onExchange.Value);
        tickerLowerToCoin = exchange.tickerLowerToCoin;
      }
      Debug.Assert(tickerLowerToCoin != null);

      if (tickerLowerToCoin.TryGetValue(ticker, out Coin coin))
      {
        return coin;
      }

      return null;
    }

    /// <summary>
    /// Only called by CrypnosticController
    /// </summary>
    /// <param name="fullName"></param>
    internal Coin(
      string fullName)
    {
      Debug.Assert(CrypnosticController.instance.fullNameLowerToCoin.ContainsKey(fullName.ToLowerInvariant()) == false);
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);
      Debug.Assert(CrypnosticController.instance.aliasLowerToCoin.ContainsKey(fullName.ToLowerInvariant()) == false);
      Debug.Assert(CrypnosticController.instance.blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()) == false);
      Debug.Assert(fullName.Equals("Ether", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("BTC", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("TetherUS", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("Tether USD", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("USDT", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("808", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("High Performance Blockch", StringComparison.InvariantCultureIgnoreCase) == false);

      this.fullName = fullName;
      this.fullNameLower = fullName.ToLowerInvariant();

      log.Trace($"Created {fullName}");
    }
    #endregion

    #region Public Read
    public TradingPair GetTradingPair(
      Coin baseCoin,
      ExchangeName exchangeName)
    {
      return tradingPairs[(exchangeName, baseCoin)];
    }

    /// <summary>
    /// True if the exchange lists the coin and is not
    /// currently reporting an issue.
    /// </summary>
    public bool IsActiveOn(
      ExchangeName exchangeName)
    {
      return CrypnosticController.instance.GetExchange(exchangeName).IsCoinActive(this);
    }
    #endregion

    #region Internal Write
    /// <summary>
    /// Creates a new pair or updates the existing one.
    /// 
    /// Use price == 0 if not known/invalid.
    /// </summary>
    internal async Task<TradingPair> AddPair(
      Exchange exchange,
      Coin baseCoin,
      decimal askPrice,
      decimal bidPrice)
    {
      Debug.Assert(exchange != null);
      Debug.Assert(baseCoin != null);
      Debug.Assert(baseCoin != this);

      try
      {
        await semaphore.WaitAsync();

        (ExchangeName, Coin) key = (exchange.exchangeName, baseCoin);
        if (tradingPairs.TryGetValue(key, out TradingPair pair))
        {
          pair.Update(askPrice, bidPrice);
        }
        else
        {
          pair = new TradingPair(exchange, baseCoin, this, askPrice, bidPrice);
          AddPair(pair);
          onNewTradingPairListed?.Invoke(this, pair);
        }

        onPriceUpdate?.Invoke(this, pair);

        return pair;
      }
      finally
      {
        semaphore.Release();
      }
    }

    /// <summary>
    /// Notes the status for a pair.
    /// 
    /// e.g. the exchange paused the book.
    /// </summary>
    internal async Task UpdatePairStatus(
      Exchange exchange,
      Coin baseCoin,
      bool isInactive)
    {
      Debug.Assert(exchange != null);
      Debug.Assert(baseCoin != this);

      try
      {
        await semaphore.WaitAsync();

        if (baseCoin == null)
        { // May be a blacklisted coin
          return;
        }

        (ExchangeName, Coin) key = (exchange.exchangeName, baseCoin);
        if (tradingPairs.TryGetValue(key, out TradingPair pair) == false)
        {
          pair = new TradingPair(exchange, baseCoin, this, 0, 0, isInactive);
          AddPair(pair);
        }

        if (pair.isInactive != isInactive)
        {
          pair.isInactive = isInactive;
          onStatusUpdate?.Invoke(this, pair);
        }
      }
      finally
      {
        semaphore.Release();
      }
    }

    void AddPair(
      TradingPair pair)
    {
      Debug.Assert(pair != null);
      Debug.Assert(pair.quoteCoin == this);
      Debug.Assert(tradingPairs.ContainsKey((pair.exchange.exchangeName, pair.baseCoin)) == false);

      tradingPairs.Add((pair.exchange.exchangeName, pair.baseCoin), pair);

      onPriceUpdate?.Invoke(this, pair);
    }
    #endregion

    #region Operators
    public static bool operator ==(
      Coin a,
      Coin b)
    {
      return a?.fullNameLower == b?.fullNameLower;
    }

    public static bool operator !=(
      Coin a,
      Coin b)
    {
      return a?.fullNameLower != b?.fullNameLower;
    }

    public override bool Equals(
      object obj)
    {
      if (obj is Coin otherCoin)
      {
        return fullNameLower == otherCoin.fullNameLower;
      }

      return false;
    }

    public override int GetHashCode()
    {
      return fullNameLower.GetHashCode();
    }

    public override string ToString()
    {
      return fullName;
    }
    #endregion
  }
}
