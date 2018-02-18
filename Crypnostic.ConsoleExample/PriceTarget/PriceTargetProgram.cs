﻿using System;
using System.Threading.Tasks;
using CrypConnect;

namespace CrypConnect.ConsoleExamples.PriceTarget
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
