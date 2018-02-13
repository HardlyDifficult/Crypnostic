using System;
using Cryptopia.API;
using Cryptopia.API.DataObjects;
using Cryptopia.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// https://www.cryptopia.co.nz/Forum/Thread/255
  /// </remarks>
  internal class CryptopiaExchange : Exchange
  {
    readonly CryptopiaApiPublic publicApi;

    readonly bool includeMaintainceStatus;

    /// <summary>
    /// 1,000 requests/minute
    /// 1,000,000 requests/day (smaller)
    /// (using half daily limit)
    /// </summary>
    /// <param name="exchangeMonitor"></param>
    /// <param name="includeMaintainceStatus"></param>
    public CryptopiaExchange(
      ExchangeMonitor exchangeMonitor,
      bool includeMaintainceStatus)
      : base(exchangeMonitor, ExchangeName.Cryptopia, 1_000_000 / 1_440)
    {
      this.includeMaintainceStatus = includeMaintainceStatus;
      publicApi = new CryptopiaApiPublic();
    }

    protected override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();
      CurrenciesResponse currenciesResponse = await publicApi.GetCurrencies();
      for (int i = 0; i < currenciesResponse.Data.Count; i++)
      {
        CurrencyResult product = currenciesResponse.Data[i];
        bool isCoinActive = true;
        if (product.ListingStatus != "Active"
          || includeMaintainceStatus == false && product.Status != "OK")
        {
          isCoinActive = false;
        }
        AddTicker(product.Symbol, Coin.FromName(product.Name), isCoinActive);
      }

      await throttle.WaitTillReady();
      TradePairsResponse tradePairsResponse = await publicApi.GetTradePairs();
      for (int i = 0; i < tradePairsResponse.Data.Count; i++)
      {
        TradePairResult tradePair = tradePairsResponse.Data[i];
        (Coin, Coin) entry = (Coin.FromName(tradePair.Currency), 
          Coin.FromName(tradePair.BaseCurrency));
        if (includeMaintainceStatus || tradePair.Status == "OK")
        {
          inactivePairs.Remove(entry);
        }
        else
        {
          inactivePairs.Add(entry);
        }
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      const string tradingPairSeparator = "/";

      await throttle.WaitTillReady();
      MarketsResponse tickerList = await publicApi.GetMarkets(new MarketsRequest());
      AddTradingPairs(tickerList.Data, (MarketResult ticker) =>
      {
        return (baseCoinTicker: ticker.Label.GetAfter(tradingPairSeparator),
        quoteCoinTicker: ticker.Label.GetBefore(tradingPairSeparator),
        askPrice: ticker.AskPrice,
        bidPrice: ticker.BidPrice);
      });
    }
  }
}
