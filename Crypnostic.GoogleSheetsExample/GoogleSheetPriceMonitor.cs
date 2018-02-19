using System;
using System.Collections.Generic;
using System.Media;
using System.Timers;
using System.Linq;
using static Crypnostic.GoogleSheetsExamples.AboutCoin;
using System.Threading.Tasks;
using Crypnostic.Tools;
using HardlyDifficult.Google.Sheets;

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
    const string arbAlarmTab = "ArbAlarm";

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


      IList<IList<object>> settings = await sheet.Read(settingsTab, "A:A");
      List<string> blacklist = new List<string>();
      foreach (var row in settings)
      {
        object blacklistData = row.FirstOrDefault();
        if (blacklistData == null)
        {
          continue;
        }
        blacklist.Add((string)blacklistData);
      }

      config.BlacklistCoins(blacklist.ToArray());

      exchangeMonitor = new CrypnosticController(config);
      await exchangeMonitor.Start();

      RefreshTimer_Elapsed(null, null);
    }

    async void RefreshTimer_Elapsed(
      object sender,
      ElapsedEventArgs e)
    {
      await DumpAllCoinsAndStats();
      await RefreshArb();
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

      await sheet.Write(dataDumpTab, "A1", results);
    }

    async Task RefreshArb()
    {
      IList<IList<object>> results = await sheet.Read(arbAlarmTab, "B:K");

      List<string[]> dataToWrite = new List<string[]>(results.Count);

      TradingPair bestBtcUsdAsk = Coin.bitcoin.FindBestOffer(Coin.usd, OrderType.Buy);
      TradingPair bestBtcUsdBid = Coin.bitcoin.FindBestOffer(Coin.usd, OrderType.Sell);
      TradingPair bestEthUsdAsk = Coin.ethereum.FindBestOffer(Coin.usd, OrderType.Buy);
      TradingPair bestEthUsdBid = Coin.ethereum.FindBestOffer(Coin.usd, OrderType.Sell);

      for (int iRow = 0; iRow < results.Count; iRow++)
      {
        dataToWrite.Add(new string[2]
        {
          "",""
        });

        IList<object> row = results[iRow];
        if (row[9]?.ToString() == "TRUE")
        {
          string coinName = row[0].ToString();
          Coin quoteCoin = Coin.FromName(coinName);
          string bidExchangeName = row[2].ToString();
          ExchangeName bidExchange = (ExchangeName)Enum.Parse(typeof(ExchangeName),
            bidExchangeName);
          string bidCurrencyName = row[3].ToString();
          Coin bidBaseCoin = Coin.FromName(bidCurrencyName);

          string askExchangeName = row[5].ToString();
          ExchangeName askExchange = (ExchangeName)Enum.Parse(typeof(ExchangeName),
            askExchangeName);
          string askCurrencyName = row[6].ToString();
          Coin askBaseCoin = Coin.FromName(askCurrencyName);

          decimal purchasePriceInBase;
          if (askBaseCoin == Coin.ethereum)
          {
            purchasePriceInBase = arbPurchasePriceETH;
          }
          else
          {
            purchasePriceInBase = arbPurchasePriceBTC;
          }

          TradingPair askPair = quoteCoin.GetTradingPair(askBaseCoin, askExchange);
          (decimal purchaseAmount, decimal quantity) = await askPair.CalcPurchasePrice(
            purchasePriceInBase: purchasePriceInBase);
          if (askBaseCoin == Coin.bitcoin)
          {
            purchaseAmount *= bestBtcUsdAsk.askPriceOrOfferYouCanBuy;
          }
          else
          {
            purchaseAmount *= bestEthUsdAsk.askPriceOrOfferYouCanBuy;
          }

          TradingPair bidPair = quoteCoin.GetTradingPair(bidBaseCoin, bidExchange);
          decimal sellAmount = await bidPair.CalcSellPrice(
            quantityOfCoin: quantity * 2);
          sellAmount /= 2;
          if (bidBaseCoin == Coin.bitcoin)
          {
            sellAmount *= bestBtcUsdBid.bidPriceOrOfferYouCanSell;
          }
          else
          {
            sellAmount *= bestEthUsdBid.bidPriceOrOfferYouCanSell;
          }

          dataToWrite[iRow][0] = purchaseAmount.ToString();
          dataToWrite[iRow][1] = sellAmount.ToString();
        }
      }


      await sheet.Write(arbAlarmTab, "L:M", dataToWrite);
    }

    async Task ConsiderAlarming()
    {
      IList<IList<object>> alarmData = await sheet.Read(settingsTab, "B2");
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

        TradingPair bestBtcUsdBid = Coin.bitcoin.FindBestOffer(Coin.usd, OrderType.Sell);
        if (bestBtcUsdBid != null)
        {
          about.columns[(int)Column.BestBidBTCUSD]
            = (bestBtcUsdBid.bidPriceOrOfferYouCanSell
            * bestBtcBid.bidPriceOrOfferYouCanSell).ToString();
        }
      }
      TradingPair bestBtcAsk = coin.FindBestOffer(Coin.bitcoin, OrderType.Buy);
      if (bestBtcAsk != null)
      {
        about.columns[(int)Column.BestAskBTC] = bestBtcAsk.askPriceOrOfferYouCanBuy.ToString();
        about.columns[(int)Column.BestAskBTCExchange] = bestBtcAsk.exchange.exchangeName.ToString();

        TradingPair bestBtcUsdAsk = Coin.bitcoin.FindBestOffer(Coin.usd, OrderType.Buy);
        if (bestBtcUsdAsk != null)
        {
          about.columns[(int)Column.BestAskBTCUSD] = (bestBtcUsdAsk.askPriceOrOfferYouCanBuy * bestBtcAsk.askPriceOrOfferYouCanBuy).ToString();
        }
      }
      TradingPair bestEthBid = coin.FindBestOffer(Coin.ethereum, OrderType.Sell);
      if (bestEthBid != null)
      {
        about.columns[(int)Column.BestBidETH] = bestEthBid.bidPriceOrOfferYouCanSell.ToString();
        about.columns[(int)Column.BestBidETHExchange] = bestEthBid.exchange.exchangeName.ToString();

        TradingPair bestEthUsdBid = Coin.ethereum.FindBestOffer(Coin.usd, OrderType.Sell);
        if (bestEthUsdBid != null)
        {
          about.columns[(int)Column.BestBidETHUSD] = (bestEthUsdBid.bidPriceOrOfferYouCanSell * bestEthBid.bidPriceOrOfferYouCanSell).ToString();
        }
      }
      TradingPair bestEthAsk = coin.FindBestOffer(Coin.ethereum, OrderType.Buy);
      if (bestEthAsk != null)
      {
        about.columns[(int)Column.BestAskETH] = bestEthAsk.askPriceOrOfferYouCanBuy.ToString();
        about.columns[(int)Column.BestAskETHExchange] = bestEthAsk.exchange.exchangeName.ToString();

        TradingPair bestEthUsdAsk = Coin.ethereum.FindBestOffer(Coin.usd, OrderType.Sell);
        if (bestEthUsdAsk != null)
        {
          about.columns[(int)Column.BestAskETHUSD] = (bestEthUsdAsk.askPriceOrOfferYouCanBuy * bestEthAsk.askPriceOrOfferYouCanBuy).ToString();
        }
      }

      TradingPair bestUsdBid = coin.FindBestOffer(Coin.usd, OrderType.Sell);
      if (bestUsdBid != null)
      {
        about.columns[(int)Column.BestBidUSD] = bestUsdBid.bidPriceOrOfferYouCanSell.ToString();
        about.columns[(int)Column.BestBidUSDExchange] = bestUsdBid.exchange.exchangeName.ToString();
      }
      TradingPair bestUsdAsk = coin.FindBestOffer(Coin.usd, OrderType.Buy);
      if (bestUsdAsk != null)
      {
        about.columns[(int)Column.BestAskUSD] = bestUsdAsk.askPriceOrOfferYouCanBuy.ToString();
        about.columns[(int)Column.BestAskUSDExchange] = bestUsdAsk.exchange.exchangeName.ToString();
      }

      if (coin.coinMarketCapData != null)
      {
        about.columns[(int)Column.MarketCapUSD] = coin.coinMarketCapData.marketCapUsd?.ToString() ?? "?";
      }

      return about;
    }
  }
}
