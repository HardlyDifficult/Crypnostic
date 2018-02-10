namespace CryptoExchanges
{
  public class TradingPair
  {
    public readonly ExchangeName exchange;
    
    public readonly string baseCoinFullName;
    public readonly string quoteCoinFullName;
    public readonly decimal AskPrice;
    public readonly decimal BidPrice;

    public TradingPair(
      ExchangeName exchange,
      string baseCoinFullName,
      string quoteCoinFullName,
      decimal AskPrice,
      decimal BidPrice)
    {
      this.exchange = exchange;
      this.baseCoinFullName = baseCoinFullName;
      this.quoteCoinFullName = quoteCoinFullName;
      this.AskPrice = AskPrice;
      this.BidPrice = BidPrice;
    }
  }
}