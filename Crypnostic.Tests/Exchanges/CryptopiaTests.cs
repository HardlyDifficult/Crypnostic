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
        return Coin.FromName("Litecoin");
      }
    }

    [TestMethod()]
    public async Task CryptopiaAllBooks()
    {
      monitor = new CrypnosticController(
        new CrypnosticConfig(exchangeName));
      await monitor.StartAsync();
      Exchange exchange = monitor.GetExchange(exchangeName);
      foreach (Coin coin in exchange.allCoins)
      {
        foreach (TradingPair pair in coin.allTradingPairs)
        {
          if (pair.isInactive == false)
          {
            pair.orderBook.autoRefresh = true;
          }
        }
      }

      TradingPair popularPair = popularQuoteCoin.GetTradingPair(popularBaseCoin, exchangeName);
      int count = 0;
      popularPair.orderBook.onUpdate += (book) =>
      {
        count++;
      };

      while (count < 2)
      {
        await Task.Delay(10);
      }
    }
  }
}