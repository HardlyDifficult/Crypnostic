using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypConnect.Tests
{
  [TestClass()]
  public class ExchangeTests : MonitorTests
  {
    [TestMethod()]
    public void AllExchanges()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig());

      int count = monitor.allCoins.Count();
      Assert.IsTrue(count > 600); // Cryptopia is the largest with about 500
    }

    [TestMethod()]
    public void AllTradingPairs()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(
          ExchangeName.Binance,
          ExchangeName.Cryptopia,
          ExchangeName.Kucoin));

      Coin omg = Coin.FromName("OmiseGO");
      int count = omg.allTradingPairs.Count();
      Assert.IsTrue(count >= 5); // ~2 each (BTC and ETH)
    }

    [TestMethod()]
    public void BitcoinPairs()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(
          ExchangeName.Binance,
          ExchangeName.Cryptopia,
          ExchangeName.Kucoin));

      Coin ark = Coin.FromName("Ark");
      TradingPair pair = Coin.bitcoin.Best(ark, true);
      Assert.IsTrue(pair == null);

      TradingPair otherPair = ark.Best(Coin.bitcoin, true);
      Assert.IsTrue(otherPair != null);
    }
  }
}