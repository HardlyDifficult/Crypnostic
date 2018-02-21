using System;
using Crypnostic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crypnostic.Tools;

namespace Crypnostic.ConsoleExamples.PriceTarget
{
  public class PriceTarget : IDisposable
  {
    #region Data
    CrypnosticController monitor;

    readonly Dictionary<Coin, decimal> coinToTargetEthPrice
      = new Dictionary<Coin, decimal>();

    decimal ethToUsd
    {
      get
      {
        return Coin.ethereum.FindBestOffer(Coin.usd, OrderType.Sell).bidPriceOrOfferYouCanSell;
      }
    }
    #endregion

    #region Init
    public PriceTarget()
    {
      CrypnosticConfig config = new CrypnosticConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin,
        ExchangeName.GDax);

      config.BlacklistCoins("Tether", "Bitcoin Cash");

      monitor = new CrypnosticController(config);
    }

    public void Start()
    {
      monitor.Start();

      AddMonitor("Monero");
      AddMonitor("OmiseGO");
    }

    void IDisposable.Dispose()
    {
      monitor.Stop();
    }
    #endregion

    #region Events
    void CoinToMonitor_onPriceUpdate(
      Coin coin,
      TradingPair pair)
    {
      TradingPair bestEthPair = coin.FindBestOffer(Coin.ethereum, OrderType.Sell);
      decimal currentValue = bestEthPair.bidPriceOrOfferYouCanSell;
      decimal goalInEth = coinToTargetEthPrice[coin];

      if (currentValue >= goalInEth)
      { // Alarm when the price increases above the goal
        decimal percentProfit = currentValue / goalInEth - 1;
        decimal usd = currentValue * ethToUsd;
        Console.WriteLine($@"

==============================================================
Price is up for {coin.fullName} ({currentValue} ETH / {usd:C}), make {percentProfit:p2}... GO GO GO!
==============================================================

");

        // Only alarm once
        coin.onPriceUpdate -= CoinToMonitor_onPriceUpdate;
      }
      else
      {
        Console.Write(".");
      }
    }
    #endregion

    #region Helpers
    void AddMonitor(
     string coinFullName)
    {
      Coin coinToMonitor = Coin.FromName(coinFullName);

      decimal goalInEth;
      { // Target a tiny price increase so that the test completes quickly
        TradingPair bestEthPair = coinToMonitor.FindBestOffer(Coin.ethereum, OrderType.Sell);
        decimal originalValue = bestEthPair.bidPriceOrOfferYouCanSell;
        goalInEth = originalValue * 1.0001m;
        decimal usd = originalValue * ethToUsd;

        Console.WriteLine($"Goal for {coinToMonitor.fullName}: {goalInEth} ETH (original: {originalValue} / {usd:C})");
      }
      coinToTargetEthPrice.Add(coinToMonitor, goalInEth);

      // Add USD (Goal: Alarm when USD and ETH are up)
      // Should just mean adding GDax

      coinToMonitor.onPriceUpdate += CoinToMonitor_onPriceUpdate;
    }
    #endregion
  }
}
