using System;

namespace CryptoExchanges.Exchanges.Binance
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
    //public string status { get; set; }
    //public float minQty { get; set; }
  }

}
