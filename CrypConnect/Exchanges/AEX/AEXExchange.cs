using System;
using System.Threading.Tasks;
using CrypConnect.Exchanges.Kucoin;
using RestSharp;
using HD;
using System.Collections.Generic;
using CrypConnect.Exchanges.AEX;

namespace CrypConnect.Exchanges
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
        Coin coin = Coin.CreateFromName(name);

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
    {// TODO remove
      return $"{quoteSymbol.ToUpperInvariant()}-{baseSymbol.ToUpperInvariant()}";
    }
  }
}
