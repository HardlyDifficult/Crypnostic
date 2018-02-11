namespace CryptoExchanges
{
  public class TradingPair
  {
    public readonly ExchangeName exchange;
    
    public readonly string baseCoinFullName;
    public readonly string quoteCoinFullName;
    public readonly decimal askPrice;
    public readonly decimal bidPrice;

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
      this.askPrice = AskPrice;
      this.bidPrice = BidPrice;
    }

    public override string ToString()
    {
      return $"{quoteCoinFullName}/{baseCoinFullName} {bidPrice}-{askPrice}";
    }
  }
}