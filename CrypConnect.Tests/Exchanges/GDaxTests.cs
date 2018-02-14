using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypConnect.Tests.Exchanges
{
  [TestClass()]
  public class GDaxTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void GDax()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.GDax);
      monitor = new ExchangeMonitor(config);
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }
  }
}