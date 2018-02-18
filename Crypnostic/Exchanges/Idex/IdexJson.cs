using System;
using System.Collections.Generic;

namespace CrypConnect.Exchanges.Idex
{

  public class IdexTickerInfoJson
  {
    public int decimals { get; set; }
    public string address { get; set; }
    public string name { get; set; }
  }

  public class IdexReturnTickerJson
  {
    public string last { get; set; }
    public string high { get; set; }
    public string low { get; set; }
    public string lowestAsk { get; set; }
    public string highestBid { get; set; }
    public string percentChange { get; set; }
    public string baseVolume { get; set; }
    public string quoteVolume { get; set; }
  }


  public class IdexRequestForMarket
  {
    public string market { get; set; }
  }

  public partial class IdexDepthListJson
  {
    public IdexDepthJson[] Asks { get; set; }
    public IdexDepthJson[] Bids { get; set; }
  }

  public partial class IdexDepthJson
  {
    //public long Timestamp { get; set; }
    public string Price { get; set; }
    public string Amount { get; set; }
    //public string Total { get; set; }
    //public string OrderHash { get; set; }
    //public IdexDepthDetailJson Params { get; set; }
  }

  //public partial class IdexDepthDetailJson
  //{
  //  //public string TokenBuy { get; set; }
  //  //public string BuySymbol { get; set; }
  //  public long? BuyPrecision { get; set; }
  //  public string AmountBuy { get; set; }
  //  //public string TokenSell { get; set; }
  //  //public string SellSymbol { get; set; }
  //  public long? SellPrecision { get; set; }
  //  public string AmountSell { get; set; }
  //  //public long? Expires { get; set; }
  //  //public long Nonce { get; set; }
  //  //public string User { get; set; }
  //  public string AmountBuyRemaining { get; set; }
  //  public string AmountSellRemaining { get; set; }
  //}

}
