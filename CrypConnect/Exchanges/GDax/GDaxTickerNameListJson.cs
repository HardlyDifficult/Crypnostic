using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges.Exchanges.GDax
{
  public class GDaxTickerNameListJson
  {
    public GDaxTickerNameJson[] Property1 { get; set; }
  }

  public class GDaxTickerNameJson
  {
    public string id { get; set; }
    public string name { get; set; }
    public string min_size { get; set; }
    public string status { get; set; }
    public object message { get; set; }
  }

}
