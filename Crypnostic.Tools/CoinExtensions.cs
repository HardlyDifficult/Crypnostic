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
      ExchangeName[] onlyOnTheseExchanges = null)
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

        if (onlyOnTheseExchanges != null)
        { // Filter by exchange (optional)
          for (int i = 0; i < onlyOnTheseExchanges.Length; i++)
          {
            if (pair.exchange.exchangeName != onlyOnTheseExchanges[i])
            {
              continue;
            }
          }
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
  }
}
