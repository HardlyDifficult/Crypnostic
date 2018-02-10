using System;

namespace CryptoExchanges.Kucoin
{
  public class KucoinMarketInfo
  {
    public bool success { get; set; }
    public string code { get; set; }
    public string msg { get; set; }
    public long timestamp { get; set; }
    public Datum[] data { get; set; }
  }

  public class Datum
  {
    public string coinType { get; set; }
    public bool trading { get; set; }
    public string symbol { get; set; }
    public float lastDealPrice { get; set; }
    public float buy { get; set; }
    public float sell { get; set; }
    public float change { get; set; }
    public string coinTypePair { get; set; }
    public int sort { get; set; }
    public float feeRate { get; set; }
    public float volValue { get; set; }
    public float high { get; set; }
    public long datetime { get; set; }
    public float vol { get; set; }
    public float low { get; set; }
    public float changeRate { get; set; }
  }
}
