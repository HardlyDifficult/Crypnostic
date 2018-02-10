using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  /// <summary>
  /// TODO rate limit
  /// </summary>
  public abstract class Exchange
  {
    public static Exchange LoadExchange(
      ExchangeName exchangeName)
    {
      switch (exchangeName)
      {
        case ExchangeName.Binance:
          return new BinanceExchange();
        case ExchangeName.Cryptopia:
          return new CryptopiaExchange();
        case ExchangeName.EtherDelta:
          return new EtherDeltaExchange();
        case ExchangeName.Kucoin:
          return new KucoinExchange();
        default:
          Debug.Fail("Missing Exchange");
          return null;
      }
    }

    public abstract Task<List<TradingPair>> GetAllTradingPairs();
  }
}
