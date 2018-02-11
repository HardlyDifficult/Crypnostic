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

    public CryptopiaExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Cryptopia)
    {
      publicApi = new CryptopiaApiPublic();
    }

    protected override async void LoadTickerNames()
    {
      // TODO consider the market status, missing from the JSON object ATM
      // TODO also do this for every other exchange
      CurrenciesResponse currenciesResponse = await publicApi.GetCurrencies();

      for (int i = 0; i < currenciesResponse.Data.Count; i++)
      {
        CurrencyResult product = currenciesResponse.Data[i];
        AddTicker(product.Symbol, product.Name);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      const string tradingPairSeparator = "/";

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
