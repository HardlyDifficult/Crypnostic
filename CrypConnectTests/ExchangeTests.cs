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
    [TestMethod()]
    public async Task WatchNanoPrice()
    {
      ExchangeMonitor monitor = new ExchangeMonitor(
        ExchangeName.Binance,
        ExchangeName.Kucoin);
      await monitor.CompleteFirstLoad();

      Coin nanoCoin = monitor.FindCoin("Nano");
      Console.WriteLine();
      //Assert.IsTrue(nanoCoin.bestAsk > 0);

      //bool changeDetected = false;
      //nanoCoin.onPriceUpdate += () => changeDetected = true;

      //while(changeDetected == false)
      //{
      //  await Task.Delay(TimeSpan.FromSeconds(1));
      //}
    }


    //[TestMethod()]
    //public async Task BinanceTradingPairs()
    //{
    //  Exchange exchange = Exchange.LoadExchange(ExchangeName.Binance);
    //  List<TradingPair> tradingPairList = await exchange.GetAllPairs();

    //  Assert.IsTrue(tradingPairList.Count > 100);
    //}

    //[TestMethod()]
    //public async Task CryptopiaTradingPairs()
    //{
    //  Exchange exchange = Exchange.LoadExchange(ExchangeName.Cryptopia);
    //  List<TradingPair> tradingPairList = await exchange.GetAllPairs();

    //  Assert.IsTrue(tradingPairList.Count > 100);
    //}

    //[TestMethod()]
    //public async Task EtherDeltaTradingPairs()
    //{
    //  Exchange exchange = Exchange.LoadExchange(ExchangeName.EtherDelta);
    //  List<TradingPair> tradingPairList = await exchange.GetAllPairs();

    //  Assert.IsTrue(tradingPairList.Count > 100);
    //}

    //[TestMethod()]
    //public async Task KucoinTradingPairs()
    //{
    //  Exchange exchange = Exchange.LoadExchange(ExchangeName.Kucoin);
    //  List<TradingPair> tradingPairList = await exchange.GetAllPairs();

    //  Assert.IsTrue(tradingPairList.Count > 100);
    //}
  }
}