using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges.Exchanges.Kucoin
{
  public class KucoinTickerNameListJson
  {
    public bool success { get; set; }
    public string code { get; set; }
    public string msg { get; set; }
    public long timestamp { get; set; }
    public KucoinTickerNameJson[] data { get; set; }
  }

  public class KucoinTickerNameJson
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

}
