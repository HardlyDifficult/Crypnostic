namespace CryptoExchanges
{
  public class TradingPair
  {
    public readonly ExchangeName exchange;
    public readonly string baseCoin;
    public readonly string quoteCoin;
    public readonly decimal AskPrice;
    public readonly decimal BidPrice;

    public TradingPair(
      ExchangeName exchange,
      string baseCoin,
      string quoteCoin,
      decimal AskPrice,
      decimal BidPrice)
    {
      this.exchange = exchange;
      this.baseCoin = baseCoin;
      this.quoteCoin = quoteCoin;
      this.AskPrice = AskPrice;
      this.BidPrice = BidPrice;
    }
  }
}