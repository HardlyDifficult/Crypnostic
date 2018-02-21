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
    public async Task BasicExchange()
    {
      CrypnosticConfig config = new CrypnosticConfig(exchangeName);
      monitor = new CrypnosticController(config);
      await monitor.StartAsync();

      Assert.IsTrue(popularQuoteCoin.hasValidTradingPairs);
    }

    [TestMethod()]
    public async Task OrderBook()
    {
      CrypnosticConfig config = new CrypnosticConfig(exchangeName);
      monitor = new CrypnosticController(config);
      await monitor.StartAsync();

      TradingPair tradingPair = popularQuoteCoin.GetTradingPair(popularBaseCoin, exchangeName);
      OrderBook orderBook = tradingPair.orderBook;
      await orderBook.RefreshAsync();
      Assert.IsTrue(orderBook.asksOrOffersYouCanBuy.Length > 0);
      Assert.IsTrue(orderBook.asksOrOffersYouCanBuy[0].price > 0);
      Assert.IsTrue(orderBook.asksOrOffersYouCanBuy[0].volume > 0);
      Assert.IsTrue(orderBook.bidsOrOffersYouCanSell.Length > 0);
      Assert.IsTrue(orderBook.bidsOrOffersYouCanSell[0].price > 0);
      Assert.IsTrue(orderBook.bidsOrOffersYouCanSell[0].volume > 0);

      Assert.IsTrue(orderBook.asksOrOffersYouCanBuy[0].price >= orderBook.bidsOrOffersYouCanSell[0].price);
      Assert.IsTrue(orderBook.asksOrOffersYouCanBuy[1].price >= orderBook.asksOrOffersYouCanBuy[0].price);
      Assert.IsTrue(orderBook.bidsOrOffersYouCanSell[0].price >= orderBook.bidsOrOffersYouCanSell[1].price);
    }



    [TestMethod()]
    public async Task OrderBookAutoUpdates()
    {
      monitor = new CrypnosticController(
        new CrypnosticConfig(exchangeName));
      await monitor.StartAsync();
      
      TradingPair tradingPair = popularQuoteCoin.GetTradingPair(popularBaseCoin, exchangeName);

      int count = 0;
      decimal lastPrice = 0;
      tradingPair.orderBook.onUpdate += (book) =>
      {
        if (book.bidsOrOffersYouCanSell[0].price != lastPrice)
        {
          lastPrice = book.bidsOrOffersYouCanSell[0].price;
          count++;
        }
      };

      while (count < 2)
      {
        await Task.Delay(10);
      }
    }

  }
}
