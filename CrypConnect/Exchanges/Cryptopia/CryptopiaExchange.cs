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
      : base(exchangeMonitor, ExchangeName.Cryptopia,
          TimeSpan.FromMilliseconds(.5 * TimeSpan.FromDays(1).TotalMilliseconds / 1_000_000))
    {
      this.includeMaintainceStatus = includeMaintainceStatus;
      publicApi = new CryptopiaApiPublic();
    }

    protected override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();
      // TODO consider the market status, missing from the JSON object ATM
      // TODO also do this for every other exchange
      CurrenciesResponse currenciesResponse = await publicApi.GetCurrencies();

      for (int i = 0; i < currenciesResponse.Data.Count; i++)
      {
        CurrencyResult product = currenciesResponse.Data[i];
        if (product.ListingStatus != "Active"
          || includeMaintainceStatus == false && product.Status != "OK")
        {
          continue;
        }
        AddTicker(product.Symbol, product.Name);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      const string tradingPairSeparator = "/";

      await throttle.WaitTillReady();
      MarketsResponse tickerList = await publicApi.GetMarkets(new MarketsRequest());
      AddTradingPairs(tickerList.Data, (MarketResult ticker) =>
      {
        return (baseCoin: ticker.Label.GetAfter(tradingPairSeparator),
        quoteCoin: ticker.Label.GetBefore(tradingPairSeparator),
        askPrice: ticker.AskPrice,
        bidPrice: ticker.BidPrice);
      });
    }
  }
}
