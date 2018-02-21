using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypnostic.Internal
{
  internal class GDaxTickerNameListJson
  {
    public GDaxTickerNameJson[] Property1 { get; set; }
  }

  internal class GDaxTickerNameJson
  {
    public string id { get; set; }
    public string name { get; set; }
    public string min_size { get; set; }
    public string status { get; set; }
    public object message { get; set; }
  }

}
