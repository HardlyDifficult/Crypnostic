using System;

namespace CryptoExchanges
{
  public class TradingPair
  {
    #region Data
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
    #endregion

    #region Init
    internal TradingPair(
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

      quoteCoin.AddPair(this);
    }
    #endregion

    #region Class Stuff
    public override string ToString()
    {
      return $"{quoteCoin}/{baseCoin} {bidPrice}-{askPrice}";
    }
    #endregion
  }
}