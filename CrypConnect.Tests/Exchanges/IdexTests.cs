using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;

namespace CrypConnect.Tests.Exchanges
{
  [TestClass()]
  public class IdexTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void Idex()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.Idex);
      monitor = new ExchangeMonitor(config);
      Coin omg = Coin.FromName("OmiseGO");
      TradingPair pair = omg.Best(Coin.ethereum, true);
      Assert.IsTrue(pair.askPrice > 0);
    }
  }
}