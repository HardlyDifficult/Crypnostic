using System;
using System.Collections.Generic;
using System.Diagnostics;
using HD;

namespace CryptoExchanges
{
  /// <summary>
  /// TODO
  ///   - how can a bot walk the list of all coins?
  ///   - how to walk all available pairs?
  ///   - how to walk order book?
  ///   (cache & consider last update time)
  /// </summary>
  public class Coin
  {
    #region Public Static 
    /// <summary>
    /// Reference to Bitcoin, for convenience.
    /// Same as new Coin("Bitcoin")
    /// </summary>
    public static readonly Coin bitcoin;

    /// <summary>
    /// Reference to Ethereum, for convenience.
    /// Same as new Coin("Ethereum")
    /// </summary>
    public static readonly Coin ethereum;
    #endregion
    
    #region Static Data
    /// <summary>
    /// Populated by the ExchangeMonitor on construction.
    /// </summary>
    internal static readonly Dictionary<string, string> aliasLowerToFullNameLower
      = new Dictionary<string, string>();

    /// <summary>
    /// Populated by the ExchangeMonitor on construction.
    /// After consider aliases.
    /// </summary>
    internal static readonly HashSet<string> blacklistedFullNameLowerList
      = new HashSet<string>();

    /// <summary>
    /// After considering aliases and blacklist.
    /// </summary>
    static readonly Dictionary<string, Coin> fullNameLowerToCoin
      = new Dictionary<string, Coin>();
    #endregion

    #region Public Data
    public readonly string fullName;

    public event Action onPriceUpdate;
    #endregion

    #region Private Data
    /// <summary>
    /// Cached in this form for performance.
    /// </summary>
    readonly string fullNameLower;

    readonly Dictionary<(ExchangeName, Coin baseCoin), TradingPair> exchangeInfo
      = new Dictionary<(ExchangeName, Coin baseCoin), TradingPair>();
    #endregion

    #region Init
    static Coin()
    {
      // Must be done in the constructor to allow the other data types to init
      bitcoin = Coin.FromName("Bitcoin");
      ethereum = Coin.FromName("Ethereum");
    }

    Coin(
      string fullName)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);
      Debug.Assert(aliasLowerToFullNameLower.ContainsKey(fullName.ToLowerInvariant()) == false);
      Debug.Assert(blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()) == false);

      this.fullName = fullName;
      this.fullNameLower = fullName.ToLowerInvariant();
    }

    public static Coin FromName(
      string fullName)
    {
      // Alias
      if (aliasLowerToFullNameLower.TryGetValue(fullName.ToLowerInvariant(),
        out string coinName))
      {
        fullName = coinName;
      }

      // Blacklist
      if (blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()))
      {
        return null;
      }

      // Existing Coin
      if (fullNameLowerToCoin.TryGetValue(fullName.ToLowerInvariant(), out Coin coin))
      {
        return coin;
      }

      // New Coin
      coin = new Coin(fullName);
      fullNameLowerToCoin.Add(fullName.ToLowerInvariant(), coin);
      return coin;
    }

    public static Coin FromTicker(
      string ticker,
      ExchangeName onExchange)
    {
      Exchange exchange = ExchangeMonitor.instance.FindExchange(onExchange);
      Debug.Assert(exchange != null);

      if (exchange.tickerLowerToCoin.TryGetValue(ticker.ToLowerInvariant(),
        out Coin coin))
      {
        return coin;
      }

      return null;
    }
    #endregion

    #region Public Write
    public void AddPair(
      TradingPair pair)
    {
      exchangeInfo[(pair.exchange.exchangeName, pair.baseCoin)] = pair;
      onPriceUpdate?.Invoke();
    }
    #endregion

    #region Public Read
    /// <summary>
    /// Finds the best offer.  Currently only supports 3 hops - do we need more?
    /// 
    /// e.g. it won't go OMG->Doge->BTC->ETH but will do OMG->Doge->BTC
    /// </summary>
    /// <param name="sellVsBuy">
    /// True: Sell this coin for baseCoin. False: Buy this coin with baseCoin.
    /// </param>
    /// <param name="baseCoinFullName"></param>
    /// <param name="exactMatchOnly">
    /// Only consider trades against the specified baseCoin.
    /// Else, consider exchange rates to find the best offer 
    /// (e.g. trading on the ETH book may lead to more BTC in the end then trading 
    /// for BTC directly)
    /// </param>
    /// <param name="exchangeName">
    /// If specified, only consider trades on this exchange
    /// </param>
    /// <returns></returns>
    public TradingPair Best(
      bool sellVsBuy,
      Coin baseCoin,
      bool exactMatchOnly = false,
      ExchangeName? exchangeName = null)
    {
      TradingPair bestPair = null;
      decimal? bestValue = null;
      foreach (KeyValuePair<(ExchangeName, Coin baseCoin), TradingPair> pair
        in exchangeInfo)
      {
        if (exchangeName != null && pair.Key.Item1 != exchangeName.Value)
        { // Filter by exchange (optional)
          continue;
        }

        if (exactMatchOnly && pair.Value.baseCoin == baseCoin)
        { // Filter by baseCoin (optional)
          continue;
        }

        decimal? value = pair.Value.GetValueIn(sellVsBuy, baseCoin);
        if (value == null)
        {
          continue;
        }


        if (bestValue == null
          || sellVsBuy && value.Value > bestValue.Value
          || sellVsBuy == false && value.Value < bestValue.Value)
        {
          bestValue = value;
          bestPair = pair.Value;
        }
      }

      return bestPair;
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
      return $"{fullName} {exchangeInfo.Count} pairs";
    }
    #endregion
  }
}
