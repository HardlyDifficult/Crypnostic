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
    /// Goal: Alarm when a coin I own becomes profitable.
    /// 
    /// TODO how-to make specific alarm profiles?
    /// </summary>
    [TestMethod()]
    public async Task AlarmOnPriceIncrease()
    {
      ExchangeMonitor monitor = new ExchangeMonitor(
        //ExchangeName.Binance,
        //ExchangeName.Cryptopia,
        ExchangeName.Kucoin);

      // TODO how-to deal with USDT vs TetherUS
      monitor.BlacklistCoins("USDT", "TetherUS", "Tether", "Bitcoin Cash");
      await monitor.CompleteFirstLoad();

      Coin coinToMonitor = monitor.FindCoin("OmiseGO");

      decimal goalInEth;
      {
        // TODO need a better format than this tuple (maybe include all pairs required)
        (TradingPair bestEthPair, decimal valueInEth) = coinToMonitor.Best(true, "Ethereum");
        Assert.IsTrue(bestEthPair.askPrice > 0);
        Assert.IsTrue(valueInEth > 0);
        // Target a tiny price increase so that the test completes quickly
        goalInEth = valueInEth * 1.0001m;
      }

      // Add USD (Goal: Alarm when USD and ETH are up)

      bool increaseDetected = false;
      coinToMonitor.onPriceUpdate += () =>
      {
        (TradingPair bestEthPair, decimal valueInEth) = coinToMonitor.Best(true, "Ethereum");

        if (valueInEth >= goalInEth)
        { // Alarm when the price increases above the goal
          increaseDetected = true;
        }
      };

      while (increaseDetected == false)
      {
        await Task.Delay(TimeSpan.FromSeconds(1));
      }
      monitor.Stop();
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