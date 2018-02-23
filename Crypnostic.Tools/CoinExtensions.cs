using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crypnostic.Tools
{
  public static class CoinExtensions
  {
    /// <summary>
    /// Scans all trading pairs for a coin to find the best offer
    /// which meets the criteria specified.
    /// </summary>
    /// <param name="onlyOnTheseExchanges">
    /// If specified, only consider trades on one of these exchanges
    /// </param>
    public static TradingPair FindBestOffer(
      this Coin quoteCoin,
      Coin baseCoin,
      OrderType orderType,
      IEnumerable<ExchangeName> onlyOnTheseExchanges)
    {
      TradingPair bestPair = null;
      decimal? bestValue = null;
      foreach (TradingPair pair in quoteCoin.allTradingPairs)
      {
        if (pair.baseCoin != baseCoin)
        { // Filter by baseCoin
          continue;
        }

        if (pair.isInactive)
        { // Ignore inactive pairs
          continue;
        }

        if (Includes(onlyOnTheseExchanges, pair.exchange.exchangeName) == false)
        { // Filter by exchange (optional)
          continue;
        }

        decimal value = orderType == OrderType.Buy
          ? pair.askPriceOrOfferYouCanBuy
          : pair.bidPriceOrOfferYouCanSell;

        if (value <= 0)
        { // No bid/ask to consider here
          continue;
        }

        if (bestValue == null
          || orderType == OrderType.Buy && value < bestValue.Value
          || orderType == OrderType.Sell && value > bestValue.Value)
        {
          bestValue = value;
          bestPair = pair;
        }
      }

      return bestPair;
    }

    static bool Includes(
      IEnumerable<ExchangeName> onlyOnTheseExchanges, ExchangeName exchange)
    {
      if (onlyOnTheseExchanges == null)
      {
        return true;
      }

      bool foundFilter = false;
      foreach (ExchangeName exchangeName in onlyOnTheseExchanges)
      {
        if (exchangeName == exchange)
        {
          return true;
        }
        foundFilter = true;
      }

      return foundFilter == false;
    }
  }
}
