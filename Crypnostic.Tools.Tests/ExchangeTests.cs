using Crypnostic.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crypnostic.Tests
{
  [TestClass()]
  public class ExchangeTests : MonitorTests
  {
    [TestMethod()]
    public async Task BitcoinPairs()
    {
      monitor = new CrypnosticController(
        new ExchangeMonitorConfig(
          ExchangeName.Binance,
          ExchangeName.Cryptopia,
          ExchangeName.Kucoin));
      await monitor.Start();

      Coin ark = Coin.FromName("Ark");
      TradingPair pair = CoinTools.Best(Coin.bitcoin, ark, true);
      Assert.IsTrue(pair == null);

      TradingPair otherPair = CoinTools.Best(ark, Coin.bitcoin, true);
      Assert.IsTrue(otherPair != null);
    }
  }
}