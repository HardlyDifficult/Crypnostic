using System;
using System.Collections.Generic;
using System.Diagnostics;
using HD;

namespace CryptoExchanges
{
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
        return Coin.FromName("Bitcoin");
      }
    }

    /// <summary>
    /// Reference to Ethereum, for convenience.
    /// </summary>
    public static Coin ethereum
    {
      get
      {
        return Coin.FromName("Ethereum");
      }
    }

    /// <summary>
    /// Reference to United States Dollar, for convenience.
    /// </summary>
    public static Coin usd
    {
      get
      {
        return Coin.FromName("United States Dollar");
      }
    }
    #endregion

    #region Public Data
    public readonly string fullName;

    public event Action<Coin> onPriceUpdate;
    #endregion

    #region Private Data
    /// <summary>
    /// Cached in this form for performance.
    /// </summary>
    readonly string fullNameLower;

    readonly Dictionary<(ExchangeName, Coin baseCoin), TradingPair> exchangeInfo
      = new Dictionary<(ExchangeName, Coin baseCoin), TradingPair>();
    #endregion

    #region Public Properties
    public IEnumerable<TradingPair> allTradingPairs
    {
      get
      {
        return exchangeInfo.Values;
      }
    }
    #endregion

    #region Init
    Coin(
      string fullName)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);
      Debug.Assert(ExchangeMonitor.instance.aliasLowerToCoin.ContainsKey(fullName.ToLowerInvariant()) == false);
      Debug.Assert(ExchangeMonitor.instance.blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()) == false);
      Debug.Assert(fullName.Equals("Ether", StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullName.Equals("BTC", StringComparison.InvariantCultureIgnoreCase) == false);

      this.fullName = fullName;
      this.fullNameLower = fullName.ToLowerInvariant();

      ExchangeMonitor.instance.fullNameLowerToCoin.Add(fullName.ToLowerInvariant(), this);
    }

    public static Coin FromName(
      string fullName)
    {
      // Alias
      if (ExchangeMonitor.instance.aliasLowerToCoin.TryGetValue(fullName.ToLowerInvariant(), out Coin coin))
      {
        return coin;
      }

      // Blacklist
      if (ExchangeMonitor.instance.blacklistedFullNameLowerList.Contains(fullName.ToLowerInvariant()))
      {
        return null;
      }

      // Existing Coin
      if (ExchangeMonitor.instance.fullNameLowerToCoin.TryGetValue(fullName.ToLowerInvariant(), out coin))
      {
        return coin;
      }

      // New Coin
      coin = new Coin(fullName);
      return coin;
    }


    public static Coin FromTicker(
      string ticker,
      ExchangeName onExchange)
    {
      ticker = ticker.ToLowerInvariant();

        Exchange exchange = ExchangeMonitor.instance.FindExchange(onExchange);
        Debug.Assert(exchange != null);

        if (exchange.tickerLowerToCoin.TryGetValue(ticker, out Coin coin))
        {
          return coin;
        }

      return null;
    }
    #endregion

    #region Public Read
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
        in exchangeInfo)
      {
        if (exchangeName != null && pair.Key.Item1 != exchangeName.Value)
        { // Filter by exchange (optional)
          continue;
        }

        if (pair.Value.baseCoin != baseCoin)
        { // Filter by baseCoin (optional)
          continue;
        }

        decimal value = sellVsBuy ? pair.Value.bidPrice : pair.Value.askPrice;
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
    #endregion

    #region Internal Write
    internal void AddPair(
      TradingPair pair)
    {
      exchangeInfo[(pair.exchange.exchangeName, pair.baseCoin)] = pair;
      onPriceUpdate?.Invoke(this);
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
