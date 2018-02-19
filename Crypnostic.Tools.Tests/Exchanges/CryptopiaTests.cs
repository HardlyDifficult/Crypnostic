using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crypnostic.Tools;

namespace Crypnostic.Tests.Exchanges
{
  [TestClass()]
  public class CryptopiaTests : MonitorTests
  {
    ExchangeName exchangeName = ExchangeName.Cryptopia;

    [TestMethod()]
    public async Task CryptopiaClosedBooks()
    {
      monitor = new CrypnosticController(
        new ExchangeMonitorConfig(exchangeName));
      await monitor.Start();
      Coin doge = Coin.FromName("Dogecoin");
      Assert.IsTrue(doge != null);

      Coin monero = Coin.FromName("Monero");
      Assert.IsTrue(monero != null);
      TradingPair pair = CoinTools.Best(monero, doge, true);
      Assert.IsTrue(pair == null || pair.isInactive);

      Coin omg = Coin.FromName("OmiseGo");
      Assert.IsTrue(omg != null);
      TradingPair omgPair = CoinTools.Best(omg, doge, true);
      Assert.IsTrue(omgPair == null || omgPair.isInactive);
    }
  }
}