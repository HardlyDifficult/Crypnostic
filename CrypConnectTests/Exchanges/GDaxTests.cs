using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptoExchanges;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchanges.Tests.Exchanges
{
  [TestClass()]
  public class GDaxTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void GDax()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.GDax);
      config.AddCoinMap(new[] { "Ethereum", "Ether" });
      monitor = new ExchangeMonitor(config);
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }
  }
}