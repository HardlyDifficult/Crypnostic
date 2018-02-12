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
    /// <summary>
    /// Goal: Alarm when reaching a price target for a specific coin.
    /// 
    /// TODO
    ///  - Monitor 2 coins 
    /// </summary>
    //[TestMethod()]
    //public async Task AlarmOnPriceIncrease()
    //{

    //}


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

    [TestMethod()]
    public async Task KucoinTradingPairs()
    {
      ExchangeMonitor monitor = new ExchangeMonitor(
        new ExchangeMonitorConfig(ExchangeName.Kucoin));
      await monitor.CompleteFirstLoad();
      Coin coin = Coin.ethereum;
      Assert.IsTrue(coin.Best(true, Coin.bitcoin) != null);
    }
  }
}