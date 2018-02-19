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
        new CrypnosticConfig(
          ExchangeName.Binance,
          ExchangeName.Cryptopia,
          ExchangeName.Kucoin));
      await monitor.Start();

      Coin ark = Coin.FromName("Ark");
      TradingPair pair = Coin.bitcoin.FindBestOffer(ark, OrderType.Sell);
      Assert.IsTrue(pair == null);

      TradingPair otherPair = ark.FindBestOffer(Coin.bitcoin, OrderType.Sell);
      Assert.IsTrue(otherPair != null);
    }
  }
}