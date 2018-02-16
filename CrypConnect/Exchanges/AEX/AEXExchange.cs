using System;
using System.Threading.Tasks;
using CrypConnect.Exchanges.Kucoin;
using RestSharp;
using HD;
using System.Collections.Generic;

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
        marketList.Add(ticker.ToLowerInvariant());
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      // TODO
      //KucoinTickerListJson tickerList = await Get<KucoinTickerListJson>("v1/open/tick");

      //foreach (KucoinTickerListJson.TickerJson ticker in tickerList.data)
      //{
      //  string baseCoinTicker = ticker.coinTypePair;
      //  string quoteCoinTicker = ticker.coinType;
      //  decimal askPrice = new decimal(ticker.sell ?? 0);
      //  decimal bidPrice = new decimal(ticker.buy ?? 0);
      //  bool isInactive = ticker.trading == false;

      //  TradingPair pair = AddTradingPair(baseCoinTicker, quoteCoinTicker, askPrice, bidPrice, isInactive);

      //  if (pair != null)
      //  {
      //    pair.lastTrade = new LastTrade(
      //      new decimal(ticker.lastDealPrice), new decimal(ticker.vol));
      //  }
      //}
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {// TODO remove
      return $"{quoteSymbol.ToUpperInvariant()}-{baseSymbol.ToUpperInvariant()}";
    }
  }
}
