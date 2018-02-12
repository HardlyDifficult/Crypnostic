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

    /// 1) Use conversion from same exchange
    /// 2) Else use conversion from a prioritized list of exchanges?
    public decimal? GetConversionTo(
      Coin targetBaseCoin,
      bool sellVsBuy)
    {
      if (this.baseCoin == targetBaseCoin)
      {
        return 1;
      }

      return exchange.GetConversion(
        quoteCoin: this.baseCoin,
        baseCoin: targetBaseCoin,
        sellVsBuy: sellVsBuy);
    }

    public decimal? GetValueIn(
      bool sellVsBuy,
      Coin baseCoin)
    {
      decimal? conversionRate = GetConversionTo(baseCoin, sellVsBuy);
      if (conversionRate == null)
      {
        return null;
      }

      decimal value = sellVsBuy ? bidPrice : askPrice;
      value *= conversionRate.Value;

      return value;
    }

    public override string ToString()
    {
      return $"{quoteCoin}/{baseCoin} {bidPrice}-{askPrice}";
    }
  }
}