using System;
using System.Collections.Generic;
using System.Media;
using System.Timers;
using System.Linq;
using static CrypConnect.GoogleSheetsExamples.AboutCoin;

namespace CrypConnect.GoogleSheetsExamples
{
  /// <summary>
  /// TODO
  ///  - Add total 24-volume seen (and total from coinmarketcap)
  ///     - or maybe the max of the two?
  /// </summary>
  public class GoogleSheetPriceMonitor
  {
    const string dataDumpTab = "CrypConnect";
    const string settingsTab = "CrypConnectSettings";

    readonly GoogleSheet sheet;

    readonly Timer refreshTimer;

    readonly ExchangeMonitor exchangeMonitor;

    public GoogleSheetPriceMonitor()
    {
      sheet = new GoogleSheet("1RoFMncCxV4ExqFQCRKSOmo-7WBSGc7F-9HjLc-5OT2c");

      ExchangeMonitorConfig config = new ExchangeMonitorConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin,
        ExchangeName.AEX,
        ExchangeName.GDax,
        ExchangeName.Idex);

      IList<IList<object>> settings = sheet.Read(settingsTab, "A:A");
      List<string> blacklist = new List<string>();
      foreach (var row in settings)
      {
        object blacklistData = row.FirstOrDefault();
        if(blacklistData == null)
        {
          continue;
        }
        blacklist.Add((string)blacklistData);
      }

      config.BlacklistCoins(blacklist.ToArray());

      exchangeMonitor = new ExchangeMonitor(config);

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
      DumpAllCoinsAndStats();

      // TODO Alarms
      //IList<IList<object>> alarmData = sheet.Read(tab, "E2:E");
      //for (int i = 0; i < alarmData.Count; i++)
      //{
      //  if(alarmData[i].Count <= 0)
      //  {
      //    continue;
      //  }
      //  if (alarmData[i][0].ToString().Equals("TRUE",
      //    StringComparison.InvariantCultureIgnoreCase))
      //  {
      //    SoundPlayer player = new SoundPlayer(
      //      @"d:\\StreamAssets\\HeyLISTEN.wav");
      //    player.Play();
      //  }
      //}

      Console.Write(".");

      refreshTimer.Start();
    }

    void DumpAllCoinsAndStats()
    {
      List<Coin> allCoinList = exchangeMonitor.allCoins.ToList();

      List<string[]> results = new List<string[]>();
      AboutCoin headers = new AboutCoin();
      headers.PopulateWithColumnNames();
      results.Add(headers.ToArray());

      for (int i = 0; i < allCoinList.Count; i++)
      {
        AboutCoin about = DescribeCoin(allCoinList[i].fullName);
        if (about != null)
        {
          results.Add(about.ToArray());
        }
      }

      for (int i = 0; i < 100; i++)
      { // Clear some old coins
        results.Add(new AboutCoin().ToArray());
      }

      sheet.Write(dataDumpTab, "A1", results);
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

      if (coin.hasValidTradingPairs == false && coin.coinMarketCapData == null)
      {
        return null;
      }

      about.columns[(int)Column.CoinName] = coin.fullName;

      TradingPair bestBtcBid = coin.Best(Coin.bitcoin, true);
      if (bestBtcBid != null)
      {
        about.columns[(int)Column.BestBidBTC] = bestBtcBid.bidPrice.ToString();
        about.columns[(int)Column.BestBidBTCExchange] = bestBtcBid.exchange.exchangeName.ToString();

        TradingPair bestBtcUsdBid = Coin.bitcoin.Best(Coin.usd, true);
        if (bestBtcUsdBid != null)
        {
          about.columns[(int)Column.BestBidBTCUSD] = (bestBtcUsdBid.bidPrice * bestBtcBid.bidPrice).ToString();
        }
      }
      TradingPair bestBtcAsk = coin.Best(Coin.bitcoin, false);
      if (bestBtcAsk != null)
      {
        about.columns[(int)Column.BestAskBTC] = bestBtcAsk.askPrice.ToString();
        about.columns[(int)Column.BestAskBTCExchange] = bestBtcAsk.exchange.exchangeName.ToString();

        TradingPair bestBtcUsdAsk = Coin.bitcoin.Best(Coin.usd, false);
        if (bestBtcUsdAsk != null)
        {
          about.columns[(int)Column.BestAskBTCUSD] = (bestBtcUsdAsk.askPrice * bestBtcAsk.askPrice).ToString();
        }
      }
      TradingPair bestEthBid = coin.Best(Coin.ethereum, true);
      if (bestEthBid != null)
      {
        about.columns[(int)Column.BestBidETH] = bestEthBid.bidPrice.ToString();
        about.columns[(int)Column.BestBidETHExchange] = bestEthBid.exchange.exchangeName.ToString();

        TradingPair bestEthUsdBid = Coin.ethereum.Best(Coin.usd, true);
        if (bestEthUsdBid != null)
        {
          about.columns[(int)Column.BestBidETHUSD] = (bestEthUsdBid.bidPrice * bestEthBid.bidPrice).ToString();
        }
      }
      TradingPair bestEthAsk = coin.Best(Coin.ethereum, false);
      if (bestEthAsk != null)
      {
        about.columns[(int)Column.BestAskETH] = bestEthAsk.askPrice.ToString();
        about.columns[(int)Column.BestAskETHExchange] = bestEthAsk.exchange.exchangeName.ToString();

        TradingPair bestEthUsdAsk = Coin.ethereum.Best(Coin.usd, false);
        if (bestEthUsdAsk != null)
        {
          about.columns[(int)Column.BestAskETHUSD] = (bestEthUsdAsk.askPrice * bestEthAsk.askPrice).ToString();
        }
      }

      TradingPair bestUsdBid = coin.Best(Coin.usd, true);
      if (bestUsdBid != null)
      {
        about.columns[(int)Column.BestBidUSD] = bestUsdBid.bidPrice.ToString();
        about.columns[(int)Column.BestBidUSDExchange] = bestUsdBid.exchange.exchangeName.ToString();
      }
      TradingPair bestUsdAsk = coin.Best(Coin.usd, false);
      if (bestUsdAsk != null)
      {
        about.columns[(int)Column.BestAskUSD] = bestUsdAsk.askPrice.ToString();
        about.columns[(int)Column.BestAskUSDExchange] = bestUsdAsk.exchange.exchangeName.ToString();
      }

      if (coin.coinMarketCapData != null)
      {
        about.columns[(int)Column.MarketCapUSD] = coin.coinMarketCapData.market_cap_usd ?? "?";
      }

      return about;
    }
  }
}
