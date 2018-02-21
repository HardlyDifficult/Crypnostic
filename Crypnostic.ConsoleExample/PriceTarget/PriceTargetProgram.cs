using System;
using System.Threading.Tasks;
using Crypnostic;

namespace Crypnostic.ConsoleExamples.PriceTarget
{
  class PriceTargetProgram
  {
    public static void Main()
    {
      using (PriceTarget priceTarget = new PriceTarget())
      {
        priceTarget.Start();
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
