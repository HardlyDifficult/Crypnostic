using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;

namespace CrypConnect.Tests.Exchanges
{
  [TestClass()]
  public class AEXTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void AEX()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.AEX);
      monitor = new ExchangeMonitor(config);
      Coin ardor = Coin.FromName("Ardor");
      TradingPair pair = ardor.Best(Coin.bitcoin, true);
      Assert.IsTrue(pair.askPrice > 0);
    }
  }
}