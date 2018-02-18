using System;
using System.Threading.Tasks;
using Crypnostic.Exchanges.Kucoin;
using RestSharp;
using HD;
using System.Collections.Generic;
using Crypnostic.Exchanges.AEX;
using System.Diagnostics;

namespace Crypnostic.Exchanges
{
  /// <remarks>
  /// https://www.aex.com/page/api_detailed.html
  /// </remarks>
  internal class AEXExchange : RestExchange
  {
    readonly IRestClient client;

    readonly HashSet<string> marketList = new HashSet<string>();

    /// <summary>
    /// Every 60 seconds the number of calls can not be more than 120 times.
    /// </summary>
    public AEXExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.AEX, 120, "https://api.aex.com")
    {
      client = new RestClient("https://www.aex.com");
      exchangeMonitor.AddAlias("BitcoinGod", "Bitcoin God");
      exchangeMonitor.AddAlias("Tether USD", "Tether");
      exchangeMonitor.AddAlias("CanYa", "CanYaCoin");
      exchangeMonitor.AddAlias("New Economy Movement", "NEM");
      exchangeMonitor.AddAlias("Lightning Bitcoin", "Lightning Bitcoin [Futures]");
      exchangeMonitor.AddAlias("UnitedBitcoin", "United Bitcoin");
    }

    public override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();
      string jsContent = await client.AsyncDownload("page/statics/js/lib/commonFun.js");
      string nameListJs = jsContent.GetBetween("all_name={", "}");
      string[] nameList = nameListJs.Split(',');
      for (int i = 0; i < nameList.Length; i++)
      {
        string tickerToName = nameList[i];
        string ticker = tickerToName.GetBefore(":");
        string name = tickerToName.GetBetween("\"", "\"");
        Coin coin = CreateFromName(name);

        // Is this possible to determine?
        bool isInactive = false;

        AddTicker(ticker, coin, isInactive);
      }

      string marketListJs = jsContent.GetBetween("alias_arr={", "}");
      string[] marketTickerList = marketListJs.Split(',');
      for (int i = 0; i < marketTickerList.Length; i++)
      {
        string marketToName = marketTickerList[i];
        string ticker = marketToName.GetBefore(":");
        Coin coin = Coin.FromTicker(ticker, exchangeName);
        if (coin != null)
        {
          marketList.Add(ticker.ToLowerInvariant());
        }
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      foreach (string market in marketList)
      {
        Coin baseCoin = Coin.FromTicker(market, exchangeName);
        if(baseCoin == null)
        {
          continue;
        }

        Dictionary<string, AexCoinJson> tickerList = await Get<Dictionary<string, AexCoinJson>>("ticker.php?c=all&mk_type=btc");

        foreach (KeyValuePair<string, AexCoinJson> ticker in tickerList)
        {
          string quoteCoinTicker = ticker.Key;
          bool isInactive;
          decimal askPrice;
          decimal bidPrice;
          if (ticker.Value.ticker == null)
          {
            isInactive = true;
            askPrice = 0;
            bidPrice = 0;
          } else
          {
            isInactive = false;
            askPrice = ticker.Value.ticker.sell;
            bidPrice = ticker.Value.ticker.buy;
          }

          TradingPair pair = AddTradingPair(baseCoin, quoteCoinTicker, askPrice, bidPrice, isInactive);

          if (pair != null && ticker.Value.ticker != null)
          {
            pair.lastTrade = new LastTrade(ticker.Value.ticker.last, 0);
          }
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      //c=btc&mk_type=btc
      return $"c={quoteSymbol.ToLowerInvariant()}&mk_type={baseSymbol.ToLowerInvariant()}";
    }

    protected override async Task<OrderBook> GetOrderBookInternal(
     string pairId)
    {
      AexDepthJson depthJson = await Get<AexDepthJson>($"depth.php?{pairId}");

      Order[] bids = ExtractOrders(depthJson.bids);
      Order[] asks = ExtractOrders(depthJson.asks);

      return new OrderBook(asks, bids);
    }

    static Order[] ExtractOrders(
      decimal[][] resultList)
    {
      Order[] orderList = new Order[resultList.Length];
      for (int i = 0; i < orderList.Length; i++)
      {
        orderList[i] = new Order(resultList[i][0], resultList[i][1]);
      }

      return orderList;
    }
  }
}
