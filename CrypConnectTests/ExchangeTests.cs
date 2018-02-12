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
    [TestMethod()]
    public async Task AlarmOnPriceIncrease()
    {
      ExchangeMonitor monitor = new ExchangeMonitor(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin);

      { // TODO move these fields into the constructor?
        // TODO how-to deal with USDT vs TetherUS (maybe a word map somewhere)
        // TODO is Tether the only one impacted?
        monitor.BlacklistCoins("USDT", "TetherUS", "Tether", "Bitcoin Cash");
        await monitor.CompleteFirstLoad();
      }

      Coin coinToMonitor = monitor.FindCoin("OmiseGO");

      decimal goalInEth;
      {
        // TODO need a better format than this tuple (maybe include all pairs required)
        ///// Begs the question, do we want to support more hops... ever?
        ///// I think if we recognize the exchanges base pairs as special this could be done.
        // TODO do we add constants for the main coins like Ether and Bitcoin?
        (TradingPair bestEthPair, decimal valueInEth) = coinToMonitor.Best(true, "Ethereum");
        Assert.IsTrue(bestEthPair.askPrice > 0);
        Assert.IsTrue(valueInEth > 0);
        // Target a tiny price increase so that the test completes quickly
        goalInEth = valueInEth * 1.0001m;
      }

      // Add USD (Goal: Alarm when USD and ETH are up)
      // Should just mean adding GDax

      bool increaseDetected = false;
      // TODO this is a verbose event (if it's an update event, maybe fire per exchange instead?)
      // Maybe in addition tothe coin price updated, there is an exchange updated event
      // Maybe use Event Profiles?
      /// -- Then a bot simply adds logic like play sound to wake me up
      /// Challenge: ExchangeMonitor may have 5 exchanges but I only want alarms on a good price from EtherDelta
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