using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Timers;

namespace CrypConnect.GoogleSheetsExamples
{
  public class GoogleSheetPriceMonitor
  {
    const string tab = "CrypConnect";

    readonly GoogleSheet sheet;

    readonly Timer refreshTimer;

    readonly ExchangeMonitor exchangeMonitor;

    public GoogleSheetPriceMonitor()
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin);
      exchangeMonitor = new ExchangeMonitor(config);

      sheet = new GoogleSheet("1RoFMncCxV4ExqFQCRKSOmo-7WBSGc7F-9HjLc-5OT2c");
      refreshTimer = new Timer()
      {
        AutoReset = false,
        Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
      };
      refreshTimer.Elapsed += RefreshTimer_Elapsed;
    }

    public void Start()
    {
      RefreshTimer_Elapsed(null, null);
    }

    void RefreshTimer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      try
      {
        IList<IList<object>> data = sheet.Read(tab, "A2:A");

        string[][] results = new string[data.Count][];
        for (int i = 0; i < results.Length; i++)
        {
          results[i] = DescribeCoin(data[i][0]);
        }
        sheet.Write(tab, "B2", results);
      }
      catch (Exception ex)
      { // TODO remove
        Console.WriteLine(ex);
      }

      IList<IList<object>> alarmData = sheet.Read(tab, "G2:G");
      for (int i = 0; i < alarmData.Count; i++)
      {
        if (alarmData[i][0].ToString().Equals("TRUE",
          StringComparison.InvariantCultureIgnoreCase))
        {
          SoundPlayer player = new SoundPlayer(
            @"d:\\StreamAssets\\HeyLISTEN.wav");
          player.Play();
        }
      }

      refreshTimer.Start();
    }

    private string[] DescribeCoin(
      object coinName)
    {
      Coin coin = Coin.FromName((string)coinName);
      if (coin == null)
      {
        return new string[] { "", "", "", "" };
      }

      TradingPair bestSell = coin.Best(Coin.bitcoin, true);
      TradingPair bestBuy = coin.Best(Coin.bitcoin, true);

      return new string[]
      {
        bestSell?.bidPrice.ToString() ?? "",
        bestSell?.exchange.exchangeName.ToString() ?? "",
        bestBuy?.askPrice.ToString() ?? "",
        bestBuy?.exchange.exchangeName.ToString() ?? "",
      };
    }
  }
}
