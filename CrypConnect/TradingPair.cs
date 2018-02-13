using System;

namespace CryptoExchanges
{
  public class TradingPair
  {
    public readonly Exchange exchange;

    public readonly Coin baseCoin;

    public readonly Coin quoteCoin;

    /// <summary>
    /// The cost to purchase.
    /// </summary>
    public readonly decimal askPrice;

    /// <summary>
    /// The price you would get by selling.
    /// </summary>
    public readonly decimal bidPrice;

    public TradingPair(
      Exchange exchange,
      Coin baseCoin,
      Coin quoteCoin,
      decimal AskPrice,
      decimal BidPrice)
    {
      this.exchange = exchange;
      this.baseCoin = baseCoin;
      this.quoteCoin = quoteCoin;
      this.askPrice = AskPrice;
      this.bidPrice = BidPrice;
    }

    public override string ToString()
    {
      return $"{quoteCoin}/{baseCoin} {bidPrice}-{askPrice}";
    }
  }
}