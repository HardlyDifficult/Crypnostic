using Cryptopia.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges
{
  public static class CryptopiaExtensions
  {
    const string tradingPairSeparator = "/";

    public static TradingPair ToTradingPair(
      this MarketResult marketResult)
    {
      return new TradingPair(
        ExchangeName.Cryptopia,
        marketResult.Label.GetAfter(tradingPairSeparator),
        marketResult.Label.GetBefore(tradingPairSeparator),
        marketResult.AskPrice,
        marketResult.BidPrice);
    }
  }
}
