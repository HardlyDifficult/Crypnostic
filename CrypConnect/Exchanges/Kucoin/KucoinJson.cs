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
      public double lastDealPrice { get; set; }
      public double buy { get; set; }
      public double sell { get; set; }
      public double change { get; set; }
      public string coinTypePair { get; set; }
      public int sort { get; set; }
      public double feeRate { get; set; }
      public double volValue { get; set; }
      public double high { get; set; }
      public long datetime { get; set; }
      public double vol { get; set; }
      public double low { get; set; }
      public double changeRate { get; set; }
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
