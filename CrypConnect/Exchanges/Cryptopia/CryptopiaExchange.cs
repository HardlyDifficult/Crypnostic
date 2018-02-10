using System;
using Cryptopia.API;
using Cryptopia.API.DataObjects;
using Cryptopia.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace CryptoExchanges
{
  internal class CryptopiaExchange : Exchange
  {
    readonly CryptopiaApiPublic publicApi;

    public CryptopiaExchange()
      : base(ExchangeName.Cryptopia)
    {
      publicApi = new CryptopiaApiPublic();
    }

    protected override async void LoadTickerNames()
    {
      CurrenciesResponse currenciesResponse = await publicApi.GetCurrencies();

      for (int i = 0; i < currenciesResponse.Data.Count; i++)
      {
        CurrencyResult product = currenciesResponse.Data[i];
        AddTicker(product.Symbol, product.Name);
      }
    }

    protected override async Task<List<TradingPair>> GetAllTradingPairs()
    {
      const string tradingPairSeparator = "/";

      MarketsResponse tickerList = await publicApi.GetMarkets(new MarketsRequest());
      return AddTradingPairs(tickerList.Data, (MarketResult ticker) =>
        (baseCoin: ticker.Label.GetAfter(tradingPairSeparator),
        quoteCoin: ticker.Label.GetBefore(tradingPairSeparator),
        askPrice: ticker.AskPrice,
        bidPrice: ticker.BidPrice));
    }
  }
}
