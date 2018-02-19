﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Crypnostic.CoinMarketCap;
using Crypnostic.Data;
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
          if (pair.Value.bidPrice > 0)
          {
            hasBid = true;
          }
          if (pair.Value.askPrice > 0)
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

      return CreateFromName(fullName);
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

    internal static Coin CreateFromName(
      string fullName)
    {
      // Blacklist
      if (CrypnosticController.instance.blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()))
      {
        return null;
      }

      // Alias
      if (CrypnosticController.instance.aliasLowerToCoin.TryGetValue(fullName.ToLowerInvariant(), out Coin coin))
      {
        return coin;
      }

      // Existing Coin
      if (CrypnosticController.instance.fullNameLowerToCoin.TryGetValue(fullName.ToLowerInvariant(), out coin))
      {
        return coin;
      }

      // New Coin
      coin = new Coin(fullName);
      return coin;
    }

    Coin(
      string fullName)
    {
      Debug.Assert(CrypnosticController.instance.fullNameLowerToCoin.ContainsKey(fullName.ToLowerInvariant()) == false);
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);
      Debug.Assert(CrypnosticController.instance.aliasLowerToCoin.ContainsKey(fullName.ToLowerInvariant()) == false);
      Debug.Assert(CrypnosticController.instance.blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()) == false);
      Debug.Assert(fullName.Equals("Ether", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("BTC", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("TetherUS", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("USDT", StringComparison.InvariantCultureIgnoreCase) == false);

      this.fullName = fullName;
      this.fullNameLower = fullName.ToLowerInvariant();

      CrypnosticController.instance.OnNewCoin(this);
    }
    #endregion

    #region Public Read
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
    internal TradingPair AddPair(
      Exchange exchange,
      Coin baseCoin,
      decimal askPrice,
      decimal bidPrice)
    {
      Debug.Assert(exchange != null);
      Debug.Assert(baseCoin != null);
      Debug.Assert(baseCoin != this);

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

    /// <summary>
    /// Notes the status for a pair.
    /// 
    /// e.g. the exchange paused the book.
    /// </summary>
    internal void UpdatePairStatus(
      Exchange exchange,
      Coin baseCoin,
      bool isInactive)
    {
      Debug.Assert(exchange != null);
      Debug.Assert(baseCoin != this);

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









































    //////////// TODO: tools below










    /// <summary>
    /// </summary>
    /// <param name="sellVsBuy">
    /// True: Sell this coin for baseCoin. False: Buy this coin with baseCoin.
    /// </param>
    /// <param name="baseCoinFullName"></param>
    /// <param name="exchangeName">
    /// If specified, only consider trades on this exchange
    /// </param>
    /// <returns></returns>
    public TradingPair Best(
      Coin baseCoin,
      bool sellVsBuy,
      ExchangeName? exchangeName = null)
    {
      TradingPair bestPair = null;
      decimal? bestValue = null;
      foreach (KeyValuePair<(ExchangeName, Coin baseCoin), TradingPair> pair
        in tradingPairs)
      {
        if (exchangeName != null && pair.Key.Item1 != exchangeName.Value)
        { // Filter by exchange (optional)
          continue;
        }

        if (pair.Value.baseCoin != baseCoin)
        { // Filter by baseCoin (optional)
          continue;
        }

        if (pair.Value.isInactive)
        { // Ignore inactive pairs
          continue;
        }

        decimal value = sellVsBuy ? pair.Value.bidPrice : pair.Value.askPrice;
        if (value <= 0)
        { // No bid/ask to consider here
          continue;
        }

        if (bestValue == null
          || sellVsBuy && value > bestValue.Value
          || sellVsBuy == false && value < bestValue.Value)
        {
          bestValue = value;
          bestPair = pair.Value;
        }
      }

      return bestPair;
    }

    // TODO no tuple?
    public async Task<(decimal purchaseAmount, decimal quantity)> CalcPurchasePrice(
      ExchangeName askExchange,
      Coin askBaseCoin,
      decimal purchasePriceInBase)
    {
      decimal purchaseAmount = 0;
      decimal quantity = 0;

      Exchange exchange = CrypnosticController.instance.GetExchange(askExchange);
      OrderBook orderBook = await exchange.GetOrderBook(this, askBaseCoin);

      for (int i = 0; i < orderBook.asks.Length; i++)
      {
        Order order = orderBook.asks[i];
        decimal purchaseAmountFromOrder = Math.Min(purchasePriceInBase,
          order.price * order.volume);

        purchaseAmount += purchaseAmountFromOrder;
        quantity += purchaseAmountFromOrder / order.price;
        purchasePriceInBase -= purchaseAmountFromOrder;

        if (purchasePriceInBase <= 0)
        {
          break;
        }
      }

      return (purchaseAmount, quantity);
    }

    public async Task<decimal> CalcSellPrice(
      ExchangeName bidExchange,
      Coin bidBaseCoin,
      decimal quantityOfCoin)
    {
      decimal sellAmount = 0;

      Exchange exchange = CrypnosticController.instance.GetExchange(bidExchange);
      OrderBook orderBook = await exchange.GetOrderBook(this, bidBaseCoin);

      for (int i = 0; i < orderBook.bids.Length; i++)
      {
        Order order = orderBook.bids[i];
        decimal sellAmountFromOrder = Math.Min(quantityOfCoin, order.volume);

        sellAmount += sellAmountFromOrder * order.price;
        quantityOfCoin -= sellAmountFromOrder;

        if (quantityOfCoin <= 0)
        {
          break;
        }
      }

      return sellAmount;
    }

  }
}
