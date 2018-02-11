using System;
using System.Collections.Generic;
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

    public Coin(
      string fullName)
    {
      this.fullName = fullName;
    }

    public void AddPair(
      TradingPair pair)
    {
      exchangeInfo[(pair.exchange, pair.baseCoinFullName)] = pair;
    }

    public override string ToString()
    {
      return $"{fullName} {exchangeInfo.Count} pairs";
    }
  }
}
