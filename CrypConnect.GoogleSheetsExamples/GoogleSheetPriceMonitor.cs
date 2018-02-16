using System;
using System.Collections.Generic;
using System.Media;
using System.Timers;
using System.Linq;

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
        ExchangeName.Kucoin,
        ExchangeName.AEX,
        ExchangeName.GDax,
        ExchangeName.Idex);
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
      IList<IList<object>> data = sheet.Read(tab, "A2:A");
      List<Coin> allCoinList = exchangeMonitor.allCoins.ToList();

      List<string[]> results = new List<string[]>();
      for (int i = 0; i < data.Count; i++)
      {
        results.Add(DescribeCoin(data[i][0], out Coin coin).ToArray());
        if (coin != null)
        {
          allCoinList.Remove(coin);
        }
      }
      for (int i = 0; i < allCoinList.Count; i++)
      { // All all remaining coins
        results.Add(DescribeCoin(allCoinList[i].fullName, out Coin coin).ToArray());
      }

      sheet.Write(tab, "A2", results);

      IList<IList<object>> alarmData = sheet.Read(tab, "E2:E");
      for (int i = 0; i < alarmData.Count; i++)
      {
        if(alarmData[i].Count <= 0)
        {
          continue;
        }
        if (alarmData[i][0].ToString().Equals("TRUE",
          StringComparison.InvariantCultureIgnoreCase))
        {
          SoundPlayer player = new SoundPlayer(
            @"d:\\StreamAssets\\HeyLISTEN.wav");
          player.Play();
        }
      }

      Console.Write(".");

      refreshTimer.Start();
    }

    List<string> DescribeCoin(
      object coinName,
      out Coin coin)
    {
      List<string> results = new List<string>();
      results.Add((string)coinName); // A
      results.Add(null); // B
      results.Add(null); // C
      results.Add(null); // D
      results.Add(null); // E
      const int bestBidBTC = 5;
      results.Add(""); // F
      const int bestBidBTCUSD = 6;
      results.Add(""); // G
      const int bestBidBTCExchange = 7;
      results.Add(""); // H
      const int bestAskBTC = 8;
      results.Add(""); // I
      const int bestAskBTCUSD = 9;
      results.Add(""); // J
      const int bestAskBTCExchange = 10;
      results.Add(""); // K
      const int bestBidETH = 11;
      results.Add(""); // L
      const int bestBidETHUSD = 12;
      results.Add(""); // M
      const int bestBidETHExchange = 13;
      results.Add(""); // N
      const int bestAskETH = 14;
      results.Add(""); // O
      const int bestAskETHUSD = 15;
      results.Add(""); // P
      const int bestAskETHExchange = 16;
      results.Add(""); // Q

      coin = Coin.FromName((string)coinName);
      if (coin == null)
      {
        return results;
      }

      TradingPair bestBtcBid = coin.Best(Coin.bitcoin, true);
      if (bestBtcBid != null)
      {
        results[bestBidBTC] = bestBtcBid.bidPrice.ToString();
        TradingPair bestBtcUsdBid = Coin.bitcoin.Best(Coin.usd, true);
        if (bestBtcUsdBid != null)
        {
          results[bestBidBTCUSD] = (bestBtcUsdBid.bidPrice * bestBtcBid.bidPrice).ToString();
        }
        results[bestBidBTCExchange] = bestBtcBid.exchange.exchangeName.ToString();
      }
      TradingPair bestBtcAsk = coin.Best(Coin.bitcoin, false);
      if (bestBtcAsk != null)
      {
        results[bestAskBTC] = bestBtcAsk.askPrice.ToString();
        TradingPair bestBtcUsdAsk = Coin.bitcoin.Best(Coin.usd, false);
        if (bestBtcUsdAsk != null)
        {
          results[bestAskBTCUSD] = (bestBtcUsdAsk.askPrice * bestBtcAsk.askPrice).ToString();
        }
        results[bestAskBTCExchange] = bestBtcAsk.exchange.exchangeName.ToString();
      }
      TradingPair bestEthBid = coin.Best(Coin.ethereum, true);
      if (bestEthBid != null)
      {
        results[bestBidETH] = bestEthBid.bidPrice.ToString();
        TradingPair bestEthUsdBid = Coin.ethereum.Best(Coin.usd, true);
        if (bestEthUsdBid != null)
        {
          results[bestBidETHUSD] = (bestEthUsdBid.bidPrice * bestEthBid.bidPrice).ToString();
        }
        results[bestBidETHExchange] = bestEthBid.exchange.exchangeName.ToString();
      }
      TradingPair bestEthAsk = coin.Best(Coin.ethereum, false);
      if (bestEthAsk != null)
      {
        results[bestAskETH] = bestEthAsk.askPrice.ToString();
        TradingPair bestEthUsdAsk = Coin.ethereum.Best(Coin.usd, false);
        if (bestEthUsdAsk != null)
        {
          results[bestAskETHUSD] = (bestEthUsdAsk.askPrice * bestEthAsk.askPrice).ToString();
        }
        results[bestAskETHExchange] = bestEthAsk.exchange.exchangeName.ToString();
      }

      return results;
    }
  }
}
