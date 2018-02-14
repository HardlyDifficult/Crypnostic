using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptoExchanges;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchanges.Tests.Exchanges
{
  [TestClass()]
  public class EtherDeltaTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void EtherDelta()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.EtherDelta);
      monitor = new ExchangeMonitor(config);
      Coin omg = Coin.FromName("OmiseGO");
      TradingPair pair = omg.Best(Coin.ethereum, true);
      Assert.IsTrue(pair.askPrice > 0);
    }
  }
}