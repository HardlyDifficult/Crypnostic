using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crypnostic.Tests.Exchanges
{
  [TestClass()]
  public class CryptopiaTests : ExchangeMonitorTests
  {
    protected override ExchangeName exchangeName
    {
      get
      {
        return ExchangeName.Cryptopia;
      }
    }

    protected override Coin popularBaseCoin
    {
      get
      {
        return Coin.bitcoin;
      }
    } 

    protected override Coin popularQuoteCoin
    {
      get
      {
        return Coin.FromName("OmiseGO");
      }
    } 

    [TestMethod()]
    public void CryptopiaClosedBooks()
    {
      monitor = new CrypnosticController(
        new ExchangeMonitorConfig(exchangeName));
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