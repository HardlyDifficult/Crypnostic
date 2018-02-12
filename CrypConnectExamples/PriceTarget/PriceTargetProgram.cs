using System;
using System.Threading.Tasks;
using CryptoExchanges;

namespace CrypConnectExamples.PriceTarget
{
  class PriceTargetProgram
  {
    ExchangeMonitor monitor;

    public static void Main()
    {
      PriceTargetProgram program = new PriceTargetProgram();
      program.Run().Wait();
    }

    public PriceTargetProgram()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(
        //ExchangeName.Binance,
        //ExchangeName.Cryptopia,
        ExchangeName.Kucoin
        );

      config.AddCoinMap(
        new[] { "TetherUS", "USDT", "Tether" },
        new[] { "TenX", "TenXPay" });

      config.BlacklistCoins("TetherUS", "Bitcoin Cash");

      monitor = new ExchangeMonitor(config);
    }

    async Task Run()
    {
      await monitor.CompleteFirstLoad();
      // TODO check on "ETHD" dupes

      Coin coinToMonitor = Coin.FromName("OmiseGO");

      decimal goalInEth;
      {
        // TODO need a better format than this tuple (maybe include all pairs required)
        ///// Begs the question, do we want to support more hops... ever?
        ///// I think if we recognize the exchanges base pairs as special this could be done.
        // TODO do we add constants for the main coins like Ether and Bitcoin?
        TradingPair bestEthPair = coinToMonitor.Best(true, Coin.ethereum);
        // Target a tiny price increase so that the test completes quickly
        decimal? originalValue = bestEthPair.GetValueIn(true, Coin.ethereum);
        goalInEth = originalValue.Value * 1.0001m;
        Console.WriteLine($"Original: {originalValue} Goal: {goalInEth}");
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
        TradingPair bestEthPair = coinToMonitor.Best(true, Coin.ethereum);
        decimal? currentValue = bestEthPair.GetValueIn(true, Coin.ethereum);
        Console.WriteLine(currentValue);
        if (currentValue >= goalInEth)
        { // Alarm when the price increases above the goal
          Console.WriteLine($"Price is up, GO GO GO!");
          increaseDetected = true;
        }
      };

      while (increaseDetected == false)
      {
        await Task.Delay(TimeSpan.FromSeconds(1));
      }
      monitor.Stop();
    }

  }
}
