using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptoExchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges.Tests
{
  [TestClass()]
  public class ExchangeTests
  {
    ExchangeMonitor monitor;

    [TestCleanup]
    public void Cleanup()
    {
      monitor.Stop();
      monitor = null;
    }

    [TestMethod()]
    public void Binance()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.Binance));
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }

    [TestMethod()]
    public void Cryptopia()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.Cryptopia));
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
      Assert.IsTrue(pair == null);
    }

    [TestMethod()]
    public void EtherDelta()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.EtherDelta));
      Assert.IsTrue(Coin.FromName("OmiseGO").Best(Coin.ethereum, true).askPrice > 0);
    }

    [TestMethod()]
    public void Kucoin()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.Kucoin));
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }
  }
}