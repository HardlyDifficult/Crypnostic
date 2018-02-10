using Binance.API.Csharp.Client.Models.Market;

namespace CryptoExchanges
{
  public static class BinanceExtensions
  {
    public static TradingPair ToTradingPair(
      this OrderBookTicker ticker)
    {
      return new TradingPair(
        ExchangeName.Binance,
        ticker.Symbol.Substring(3),
        ticker.Symbol.Substring(0, 3),
        ticker.AskPrice,
        ticker.BidPrice);
    }
  }
}
