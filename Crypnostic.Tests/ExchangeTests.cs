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
    public async Task AllExchanges()
    {
      monitor = new CrypnosticController(
        new ExchangeMonitorConfig());
      await monitor.Start();

      int count = monitor.allCoins.Count();
      Assert.IsTrue(count > 600); // Cryptopia is the largest with about 500
    }

    [TestMethod()]
    public async Task AllTradingPairs()
    {
      monitor = new CrypnosticController(
        new ExchangeMonitorConfig(
          ExchangeName.Binance,
          ExchangeName.Cryptopia,
          ExchangeName.Kucoin));
      await monitor.Start();

      Coin omg = Coin.FromName("OmiseGO");
      int count = omg.allTradingPairs.Count();
      Assert.IsTrue(count >= 5); // ~2 each (BTC and ETH)
    }

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
      TradingPair pair = Coin.bitcoin.Best(ark, true);
      Assert.IsTrue(pair == null);

      TradingPair otherPair = ark.Best(Coin.bitcoin, true);
      Assert.IsTrue(otherPair != null);
    }
  }
}