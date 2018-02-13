using System;
using System.Threading.Tasks;
using CryptoExchanges;

namespace CrypConnectExamples.PriceTarget
{
  class PriceTargetProgram
  {
    public static void Main()
    {
      using (PriceTarget priceTarget = new PriceTarget())
      {
        while (true)
        {
          if (Console.ReadLine().Equals("Quit", StringComparison.InvariantCultureIgnoreCase))
          {
            break;
          }
        }
      }
    }
  }
}
