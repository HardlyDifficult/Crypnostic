using System;

namespace CryptoExchanges
{
  public class TradingPair
  {
    public readonly Exchange exchange;
    
    public readonly string baseCoinFullName;
    public readonly string quoteCoinFullName;
    
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

    // 0.001 BTC / SPANK (current Pair) -> 10 BNB / SPANK (goal)
    /// 1) Use conversion from same exchange
    /// 2) Else use conversion from a prioritized list of exchanges?
    public decimal? GetConversionTo(
      string baseCoinFullName,
      bool sellVsBuy)
    {
      if(this.baseCoinFullName.Equals(baseCoinFullName, 
        StringComparison.InvariantCultureIgnoreCase))
      {
        return 1;
      }
      return exchange.GetConversion(
        fromOrQuoteCoinFullName: this.baseCoinFullName, // e.g. ETH
        toOrBaseCoinFullName: baseCoinFullName,// e.g. BTC
        sellVsBuy: sellVsBuy); 
    }
  }
}