using System;
using CryptoExchanges;
using System.Collections.Generic;

namespace CrypConnectExamples.PriceTarget
{
  public class PriceTarget : IDisposable
  {
    #region Data
    ExchangeMonitor monitor;

    readonly Dictionary<Coin, decimal> coinToTargetEthPrice
      = new Dictionary<Coin, decimal>();

    decimal ethToUsd
    {
      get
      {
        return Coin.ethereum.Best(Coin.usd, sellVsBuy: true).bidPrice;
      }
    }
    #endregion

    #region Init
    public PriceTarget()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin,
        ExchangeName.GDax);

      config.AddCoinMap(
        new[] { "Ethereum", "Ether" },
        new[] { "TetherUS", "USDT", "Tether" },
        new[] { "TenX", "TenXPay" });

      config.BlacklistCoins("TetherUS", "Bitcoin Cash");

      monitor = new ExchangeMonitor(config);

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
      Coin coin)
    {
      TradingPair bestEthPair = coin.Best(Coin.ethereum, sellVsBuy: true);
      decimal currentValue = bestEthPair.bidPrice;
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
        TradingPair bestEthPair = coinToMonitor.Best(Coin.ethereum, sellVsBuy: true);
        decimal originalValue = bestEthPair.bidPrice;
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
