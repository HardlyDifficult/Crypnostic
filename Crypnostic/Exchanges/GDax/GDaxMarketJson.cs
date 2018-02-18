using System;

namespace Crypnostic.Exchanges.GDax
{

  public class GDaxProductListJson
  {
    public GDaxProductJson[] data { get; set; }
  }

  public class GDaxProductJson
  {
    public string id { get; set; }
    public string base_currency { get; set; }
    public string quote_currency { get; set; }
    public string base_min_size { get; set; }
    public string base_max_size { get; set; }
    public string quote_increment { get; set; }
    public string status { get; set; }
  }

  public class GDaxProductTickerJson
  {
    public int trade_id { get; set; }
    public string price { get; set; }
    public string size { get; set; }
    public string bid { get; set; }
    public string ask { get; set; }
    public string volume { get; set; }
    public DateTime time { get; set; }
  }


  public class GDaxDepthListJson
  {
    //public long sequence { get; set; }
    public string[][] bids { get; set; }
    public string[][] asks { get; set; }
  }
}
