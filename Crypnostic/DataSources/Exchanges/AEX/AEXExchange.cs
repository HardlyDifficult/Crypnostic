using System;
using System.Threading.Tasks;
using RestSharp;
using HD;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crypnostic.Internal
{
  /// <remarks>
  /// https://www.aex.com/page/api_detailed.html
  /// </remarks>
  internal class AEXExchange : RestExchange
  {
    readonly IRestClient wwwClient;

    readonly HashSet<string> marketList = new HashSet<string>();

    /// <summary>
    /// Every 60 seconds the number of calls can not be more than 120 times.
    /// </summary>
    public AEXExchange()
      : base(ExchangeName.AEX, "https://api.aex.com", 120)
    {
      // Two base URLs are required for this exchange
      wwwClient = new RestClient("https://www.aex.com");

      CrypnosticController.instance.AddCoinAlias(
        new[] { "Bitcoin God", "BitcoinGod" },
        new[] { "Tether", "Tether USD" },
        new[] { "CanYaCoin", "CanYa" },
        new[] { "NEM", "New Economy Movement" },
        new[] { "Lightning Bitcoin [Futures]", "Lightning Bitcoin" },
        new[] { "United Bitcoin", "UnitedBitcoin" });
    }

    protected override async Task RefreshTickers()
    {
      await throttle.WaitTillReady();
      string jsContent = await wwwClient.AsyncDownload("page/statics/js/lib/commonFun.js");
      string nameListJs = jsContent.GetBetween("all_name={", "}");
      string[] nameList = nameListJs.Split(',');
      for (int i = 0; i < nameList.Length; i++)
      {
        string tickerToName = nameList[i];
        string ticker = tickerToName.GetBefore(":");
        string name = tickerToName.GetBetween("\"", "\"");
        Coin coin = await CreateFromName(name);

        // Is this possible to determine?
        bool isInactive = false;

        await AddTicker(coin, ticker, isInactive);
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

    protected override async Task RefreshTradingPairs()
    {
      foreach (string market in marketList)
      {
        Coin baseCoin = Coin.FromTicker(market, exchangeName);
        if(baseCoin == null)
        {
          continue;
        }

        Dictionary<string, AexCoinJson> tickerList = await Get<Dictionary<string, AexCoinJson>>("ticker.php?c=all&mk_type=btc");
        if(tickerList == null)
        {
          return;
        }

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

          TradingPair pair = await AddTradingPair(quoteCoinTicker, baseCoin, askPrice, bidPrice, isInactive);

          if (pair != null && ticker.Value.ticker != null)
          {
            pair.lastTrade.Update(ticker.Value.ticker.last);
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

    protected override async Task UpdateOrderBook(
     string pairId,
     OrderBook orderBook)
    {
      AexDepthJson depthJson = await Get<AexDepthJson>($"depth.php?{pairId}");
      if(depthJson == null)
      {
        return;
      }

      Order[] bids = ExtractOrders(depthJson.bids);
      Order[] asks = ExtractOrders(depthJson.asks);

      orderBook.Update(asks, bids);
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
