using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypnostic.Tests
{
  public abstract class ExchangeMonitorTests : MonitorTests
  {
    protected abstract ExchangeName exchangeName
    {
      get;
    }
    
    protected abstract Coin popularQuoteCoin
    {
      get;
    }

    protected abstract Coin popularBaseCoin
    {
      get;
    }

    [TestMethod()]
    public void BasicExchange()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(exchangeName);
      monitor = new CrypnosticController(config);
      TradingPair pair = popularQuoteCoin.Best(popularBaseCoin, true);
      Assert.IsTrue(pair.askPrice > 0);
    }

    [TestMethod()]
    public async Task OrderBook()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(exchangeName);
      monitor = new CrypnosticController(config);
      Exchange exchange = monitor.FindExchange(exchangeName);

      OrderBook orderBook = await exchange.GetOrderBook(popularQuoteCoin, popularBaseCoin);
      Assert.IsTrue(orderBook.asks.Length > 0);
      Assert.IsTrue(orderBook.asks[0].price > 0);
      Assert.IsTrue(orderBook.asks[0].volume > 0);
      Assert.IsTrue(orderBook.bids.Length > 0);
      Assert.IsTrue(orderBook.bids[0].price > 0);
      Assert.IsTrue(orderBook.bids[0].volume > 0);

      Assert.IsTrue(orderBook.asks[0].price >= orderBook.bids[0].price);
      Assert.IsTrue(orderBook.asks[1].price >= orderBook.asks[0].price);
      Assert.IsTrue(orderBook.bids[0].price >= orderBook.bids[1].price);
    }

  }
}
