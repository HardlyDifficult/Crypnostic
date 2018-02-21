using System;

namespace Crypnostic
{
  public class BinanceProductListJson
  {
    public BinanceProductJson[] data { get; set; }
  }

  public class BinanceProductJson
  {
    public string quoteAsset { get; set; }
    public string quoteAssetName { get; set; }
    public string baseAsset { get; set; }
    public string baseAssetName { get; set; }

    //public string symbol { get; set; }
    //public float tradedMoney { get; set; }
    //public string baseAssetUnit { get; set; }
    //public string tickSize { get; set; }
    //public float prevClose { get; set; }
    //public int activeBuy { get; set; }
    //public string high { get; set; }
    //public int lastAggTradeId { get; set; }
    //public string low { get; set; }
    //public string matchingUnitType { get; set; }
    //public string close { get; set; }
    //public object productType { get; set; }
    //public bool active { get; set; }
    //public float minTrade { get; set; }
    //public float activeSell { get; set; }
    //public string withdrawFee { get; set; }
    //public string volume { get; set; }
    //public int decimalPlaces { get; set; }
    //public string quoteAssetUnit { get; set; }
    //public string open { get; set; }
    public string status { get; set; }
    //public float minQty { get; set; }
  }



  public class BinanceTradeJson
  {
    public int id { get; set; }
    public string price { get; set; }
    public string qty { get; set; }
    public long time { get; set; }
    public bool isBuyerMaker { get; set; }
    public bool isBestMatch { get; set; }
  }


  public class BinanceDepthJson
  {
    //public int lastUpdateId { get; set; }
    public object[][] bids { get; set; }
    public object[][] asks { get; set; }
  }
}
