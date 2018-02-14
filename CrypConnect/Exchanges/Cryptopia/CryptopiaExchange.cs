using System;
using Cryptopia.API;
using Cryptopia.API.DataObjects;
using Cryptopia.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CrypConnect
{
  /// <remarks>
  /// https://www.cryptopia.co.nz/Forum/Thread/255
  /// </remarks>
  internal class CryptopiaExchange : Exchange
  {
    // TODO remove and switch to 'RestExchange'
    readonly CryptopiaApiPublic publicApi;

    /// <summary>
    /// 1,000 requests/minute
    /// 1,000,000 requests/day (smaller)
    /// (using half daily limit)
    /// </summary>
    /// <param name="exchangeMonitor"></param>
    public CryptopiaExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Cryptopia, 1_000_000 / 1_440)
    {
      publicApi = new CryptopiaApiPublic();
    }

    public override async Task LoadTickerNames()
    {
      await throttle.WaitTillReady();
      CurrenciesResponse currenciesResponse = await publicApi.GetCurrencies();
      for (int i = 0; i < currenciesResponse.Data.Count; i++)
      {
        CurrencyResult product = currenciesResponse.Data[i];
        bool isCoinActive = true;
        if (product.ListingStatus != "Active" || product.Status != "OK")
        {
          isCoinActive = false;
        }
        AddTicker(product.Symbol, Coin.CreateFromName(product.Name, true), isCoinActive);
      }

      await throttle.WaitTillReady();
      TradePairsResponse tradePairsResponse = await publicApi.GetTradePairs();
      for (int i = 0; i < tradePairsResponse.Data.Count; i++)
      {
        TradePairResult tradePair = tradePairsResponse.Data[i];
        (Coin, Coin) entry = (Coin.CreateFromName(tradePair.Currency, true),
          Coin.CreateFromName(tradePair.BaseCurrency, true));
        entry.Item1.UpdatePairStatus(this, entry.Item2, tradePair.Status != "OK");
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      const string tradingPairSeparator = "/";

      await throttle.WaitTillReady();
      MarketsResponse tickerList = await publicApi.GetMarkets(new MarketsRequest());
      foreach (MarketResult ticker in tickerList.Data)
      {
        AddTradingPair(baseCoinTicker: ticker.Label.GetAfter(tradingPairSeparator),
          quoteCoinTicker: ticker.Label.GetBefore(tradingPairSeparator),
          askPrice: ticker.AskPrice,
          bidPrice: ticker.BidPrice);
      }
    }
  }
}
