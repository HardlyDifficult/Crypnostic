//using System;
//using RestSharp;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using HD;
//using Crypnostic.DataSources.Exchanges;

//namespace Crypnostic
//{
//  /// <summary>
//  /// TODO EtherDelta lastTrade
//  /// </summary>
//  /// <remarks>
//  /// https://github.com/etherdelta/etherdelta.github.io/blob/master/docs/API_OLD.md
//  /// </remarks>
//  internal class EtherDeltaExchange : RestExchange
//  {
//    public override bool supportsOverlappingBooks
//    {
//      get
//      {
//        return true;
//      }
//    }

//    /// <summary>
//    /// No stated throttle limit, going with the same as Crytpopia
//    /// </summary>
//    /// <param name="exchangeMonitor"></param>
//    public EtherDeltaExchange(
//      ExchangeMonitor exchangeMonitor)
//      : base(exchangeMonitor, ExchangeName.EtherDelta, 1_000_000 / 1_440,
//          "https://api.etherdelta.com")
//    {
//    }

//    public override async Task LoadTickerNames()
//    {
//      // how to LoadTickerNames from EtherDelta?
//      await Task.Delay(0);
//    }

//    protected override async Task GetAllTradingPairs()
//    {
//      Dictionary<string, Dictionary<string, object>> tickerList;
//      while (true)
//      {
//        try
//        {
//          tickerList =
//            await Get<Dictionary<string, Dictionary<string, object>>>("returnTicker");

//          if (tickerList != null)
//          {
//            break;
//          }
//        }
//        catch
//        {
//          await Task.Delay(3500 + ExchangeMonitor.instance.random.Next(2000));
//        }
//      }

//      foreach (var ticker in tickerList)
//      {
//        AddTradingPair(baseCoinTicker: "ETH",
//          quoteCoinTicker: ticker.Key.GetAfter("_"),
//          askPrice: Convert.ToDecimal(ticker.Value["ask"]),
//          bidPrice: Convert.ToDecimal(ticker.Value["bid"]), 
//          isInactive: false);
//      }
//    }

//    protected override string GetPairId(
//      string quoteSymbol, 
//      string baseSymbol)
//    {
//      return $"{quoteSymbol.ToUpperInvariant()}-{baseSymbol.ToUpperInvariant()}";
//    }

//    protected override Task<OrderBook> GetOrderBookInternal(
//      string pairId)
//    {
//      // TODO
//      return null;
//    }
//  }
//}
