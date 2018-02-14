using System;

namespace CrypConnect.Exchanges.Kucoin
{
  public class KucoinTickerListJson
  {
    public class TickerJson
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

    public bool success { get; set; }
    public string code { get; set; }
    public string msg { get; set; }
    public long timestamp { get; set; }
    public TickerJson[] data { get; set; }
  }

  public class KucoinProductListJson
  {
    public class ProductJson
    {
      public float withdrawMinFee { get; set; }
      public float withdrawMinAmount { get; set; }
      public float withdrawFeeRate { get; set; }
      public int confirmationCount { get; set; }
      public string withdrawRemark { get; set; }
      public object infoUrl { get; set; }
      public string name { get; set; }
      public int tradePrecision { get; set; }
      public string depositRemark { get; set; }
      public bool enableWithdraw { get; set; }
      public bool enableDeposit { get; set; }
      public string coin { get; set; }
    }

    public bool success { get; set; }
    public string code { get; set; }
    public string msg { get; set; }
    public long timestamp { get; set; }
    public ProductJson[] data { get; set; }
  }

}
