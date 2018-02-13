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
    public void GDax()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(ExchangeName.GDax);
      config.AddCoinMap(
        new[] { "Ethereum", "Ether" });

      monitor = new ExchangeMonitor(config);
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

      Coin omg = Coin.FromName("OmiseGo");
      Assert.IsTrue(omg != null);
      TradingPair omgPair = omg.Best(doge, true);
      Assert.IsTrue(omgPair == null);
    }

    [TestMethod()]
    public void EtherDelta()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.EtherDelta));
      Coin omg = Coin.FromName("OmiseGO");
      TradingPair pair = omg.Best(Coin.ethereum, true);
      Assert.IsTrue(pair.askPrice > 0);
    }

    [TestMethod()]
    public void Kucoin()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.Kucoin));
      Assert.IsTrue(Coin.ethereum.Best(Coin.bitcoin, true).askPrice > 0);
    }

    [TestMethod()]
    public void AllCoins()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(
          ExchangeName.Binance,
          ExchangeName.Cryptopia,
          ExchangeName.Kucoin));

      int count = Coin.allCoins.Count();
      Assert.IsTrue(count > 600); // Cryptopia is the largest with about 500
    }

    [TestMethod()]
    public void AllExchanges()
    {
      monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig());

      int count = Coin.allCoins.Count();
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



  }
}