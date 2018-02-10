using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  public abstract class Exchange
  {
    public static Exchange LoadExchange(
      ExchangeName exchangeName)
    {
      switch (exchangeName)
      {
        case ExchangeName.Cryptopia:
          return new CryptopiaExchange();
        case ExchangeName.Binance:
          return new BinanceExchange();
        default:
          Debug.Fail("Missing Exchange");
          return null;
      }
    }

    public abstract Task<List<TradingPair>> GetAllTradingPairs();
  }
}
