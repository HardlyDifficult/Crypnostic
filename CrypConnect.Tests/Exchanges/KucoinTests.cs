using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypConnect.Tests.Exchanges
{
  [TestClass()]
  public class KucoinTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void Kucoin()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.Kucoin);
      monitor = new ExchangeMonitor(config);
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }
  }
}