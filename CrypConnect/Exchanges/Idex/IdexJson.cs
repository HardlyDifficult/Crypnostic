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


}
