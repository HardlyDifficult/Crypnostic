using System;
using System.Collections.Generic;
using System.Media;
using System.Timers;
using System.Linq;
using static Crypnostic.GoogleSheetsExamples.AboutCoin;
using System.Threading.Tasks;
using Crypnostic.Tools;
using HardlyDifficult.Google.Sheets;
using HD;

namespace Crypnostic.GoogleSheetsExamples
{
  /// <summary>
  /// TODO
  ///  - Add buy targets
  ///  - Add total 24-volume seen (and total from coinmarketcap)
  ///     - or maybe the max of the two?
  /// </summary>
  public class GoogleSheetPriceMonitor
  {
    const string dataDumpTab = "Crypnostic";
    const string settingsTab = "CrypnosticSettings";

    decimal minCap;

    // TODO USD goal
    const decimal arbPurchasePriceETH = 1m;
    const decimal arbPurchasePriceBTC = .1m;

    readonly GoogleSheet sheet;

    readonly Timer refreshTimer;

    CrypnosticController exchangeMonitor;

    public GoogleSheetPriceMonitor()
    {
      sheet = new GoogleSheet("Crypnostic Example",
        "1jEEQF_gAHFSQPOS9mE3HkOwA7HOdpxFf6np9MLJtpyY");

      refreshTimer = new Timer()
      {
        AutoReset = false,
        Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
      };
      refreshTimer.Elapsed += RefreshTimer_Elapsed;
    }

    public async Task Start()
    {
      CrypnosticConfig config = new CrypnosticConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin,
        //ExchangeName.AEX,
        ExchangeName.GDax,
        ExchangeName.Idex
        );


      IList<IList<object>> settingsBlacklist = await sheet.Read(settingsTab, "A:A");
      if (settingsBlacklist != null)
      {
        List<string> blacklist = new List<string>();
        foreach (var row in settingsBlacklist)
        {
          object blacklistData = row.FirstOrDefault();
          if (blacklistData == null)
          {
            continue;
          }
          blacklist.Add((string)blacklistData);
        }

        config.BlacklistCoins(blacklist.ToArray());
      }

      IList<IList<object>> settingsMinCap = await sheet.Read(settingsTab, "C2");
      if(settingsMinCap != null && settingsMinCap.Count > 0 && settingsMinCap[0].Count > 0)
      {
        minCap = decimal.Parse(settingsMinCap[0][0].ToString().RemoveCruftFromNumber());
      }

      exchangeMonitor = new CrypnosticController(config);
      await exchangeMonitor.Start();

      RefreshTimer_Elapsed(null, null);
    }

    async void RefreshTimer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      await DumpAllCoinsAndStats();
      await ConsiderAlarming();

      Console.Write(".");

      refreshTimer.Start();
    }

    async Task DumpAllCoinsAndStats()
    {
      List<Coin> allCoinList = exchangeMonitor.allCoins.ToList();

      List<string[]> results = new List<string[]>();
      AboutCoin headers = new AboutCoin();
      headers.PopulateWithColumnNames();
      results.Add(headers.ToArray());

      for (int i = 0; i < allCoinList.Count; i++)
      {
        Coin coin = allCoinList[i];
        if(coin.coinMarketCapData == null 
          || coin.coinMarketCapData.marketCapUsd == null
          || coin.coinMarketCapData.marketCapUsd.Value < minCap)
        {
          continue;
        }

        AboutCoin about = DescribeCoin(coin.fullName);
        if (about != null)
        {
          results.Add(about.ToArray());
        }
      }

      for (int i = 0; i < 100; i++)
      { // Clear some old coins
        results.Add(new AboutCoin().ToArray());
      }

      await sheet.Write(dataDumpTab, "A1", results);
    }

    async Task ConsiderAlarming()
    {
      IList<IList<object>> alarmData = await sheet.Read(settingsTab, "B2");
      if(alarmData == null)
      {
        return;
      }

      if (alarmData.Count > 0
        && alarmData[0].Count > 0
        && alarmData[0][0] != null)
      {
        if (int.TryParse(alarmData[0][0].ToString(), out int alarmCount))
        {
          if (alarmCount > 0)
          {
            SoundPlayer player = new SoundPlayer(
            @"d:\\StreamAssets\\HeyLISTEN.wav");
            player.Play();
          }
        }
      }
    }

    AboutCoin DescribeCoin(
      object coinName)
    {
      AboutCoin about = new AboutCoin();

      Coin coin = Coin.FromName((string)coinName);
      if (coin == null)
      {
        return about;
      }

      if (coin.hasValidTradingPairs == false) //&& coin.coinMarketCapData == null)
      {
        return null;
      }

      about.columns[(int)Column.CoinName] = coin.fullName;

      TradingPair bestBtcBid = coin.FindBestOffer(Coin.bitcoin, OrderType.Sell);
      if (bestBtcBid != null)
      {
        about.columns[(int)Column.BestBidBTC] = bestBtcBid.bidPriceOrOfferYouCanSell.ToString();
        about.columns[(int)Column.BestBidBTCExchange]
          = bestBtcBid.exchange.exchangeName.ToString();
      }
      TradingPair bestBtcAsk = coin.FindBestOffer(Coin.bitcoin, OrderType.Buy);
      if (bestBtcAsk != null)
      {
        about.columns[(int)Column.BestAskBTC] = bestBtcAsk.askPriceOrOfferYouCanBuy.ToString();
        about.columns[(int)Column.BestAskBTCExchange] = bestBtcAsk.exchange.exchangeName.ToString();
      }

      if (coin.coinMarketCapData != null)
      {
        about.columns[(int)Column.MarketCapUSD] = coin.coinMarketCapData.marketCapUsd?.ToString() ?? "?";
      }

      return about;
    }
  }
}
