using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptoExchanges;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchanges.Tests.Exchanges
{
  [TestClass()]
  public class CryptopiaTests : ExchangeMonitorTests
  {
    [TestMethod()]
    public void Cryptopia()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.Cryptopia);
      monitor = new ExchangeMonitor(config);
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }

    [TestMethod()]
    public void CryptopiaClosedBooks()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.Cryptopia));
      Coin doge = Coin.FromName("Dogecoin");
      Assert.IsTrue(doge != null);

      Coin monero = Coin.FromName("Monero");
      Assert.IsTrue(monero != null);
      TradingPair pair = monero.Best(doge, true);
      Assert.IsTrue(pair == null || pair.isInactive);

      Coin omg = Coin.FromName("OmiseGo");
      Assert.IsTrue(omg != null);
      TradingPair omgPair = omg.Best(doge, true);
      Assert.IsTrue(omgPair == null || omgPair.isInactive);
    }
  }
}