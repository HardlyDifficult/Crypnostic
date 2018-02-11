using System;
using System.Collections.Generic;
using System.Diagnostics;
using HD;

namespace CryptoExchanges
{
  public class Coin
  {
    public readonly string fullName;

    // 1) best buy and sell price from each supported exchange
    // 2) ability to review the order book for each exchange
    // - both need to consider last update time
    // Events for the coin as well as higher level

    readonly Dictionary<(ExchangeName, string baseCoinFullName), TradingPair> exchangeInfo
      = new Dictionary<(ExchangeName, string baseCoinFullName), TradingPair>();


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
      string baseCoinFullName,
      bool exactMatchOnly = false,
      ExchangeName? exchangeName = null)
    {
      TradingPair bestPair = null;
      decimal? bestValue = null;
      foreach (KeyValuePair<(ExchangeName, string baseCoinFullName), TradingPair> pair
        in exchangeInfo)
      {
        if (exchangeName != null && pair.Key.Item1 != exchangeName.Value)
        { // Filter by exchange (optional)
          continue;
        }

        if (exactMatchOnly && pair.Value.baseCoinFullName.Equals(baseCoinFullName,
          StringComparison.InvariantCultureIgnoreCase) == false)
        { // Filter by baseCoin (optional)
          continue;
        }

        decimal value = sellVsBuy ? pair.Value.bidPrice : pair.Value.askPrice;
        // e.g. BNB on Cryptopia
        // quoteCoin = SPANK (this coin is SPANK)
        // baseCoin = BNB
        // 
        // 
        decimal? conversionRate = pair.Value.GetConversionTo(baseCoinFullName, sellVsBuy);
        if(conversionRate == null)
        {
          continue;
        }

        string test = $"1 {fullName}=={value} {pair.Value.baseCoinFullName}. {value} {pair.Value.baseCoinFullName}=={value * conversionRate} {baseCoinFullName}";
        value *= conversionRate.Value;
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

    public Coin(
      string fullName)
    {
      this.fullName = fullName;
    }

    public void AddPair(
      TradingPair pair)
    {
      exchangeInfo[(pair.exchange.exchangeName, pair.baseCoinFullName)] = pair;
    }

    public override string ToString()
    {
      return $"{fullName} {exchangeInfo.Count} pairs";
    }
  }
}
