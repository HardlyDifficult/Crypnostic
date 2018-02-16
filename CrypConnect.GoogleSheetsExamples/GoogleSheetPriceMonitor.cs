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
  ///  - Don't show coins with no pairs
  ///  - What's up with the nzdollar and other markets i don't care for?
  ///  - Ingest blacklist from sheets
  ///  - Confirm Bitcoin and Ether
  /// </summary>
  public class GoogleSheetPriceMonitor
  {
    const string tab = "CrypConnect";

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

      config.BlacklistCoins(
        "Bitcoin Cash",
        "Bitcoin God", 
        "NZed",
        "British Pound",
        "Tether",
        "Euro");

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

      sheet.Write(tab, "A1", results);
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

      if(coin.hasValidTradingPairs == false)
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

      return about;
    }
  }
}
