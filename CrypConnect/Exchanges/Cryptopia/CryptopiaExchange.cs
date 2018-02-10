using Cryptopia.API;
using Cryptopia.API.DataObjects;
using Cryptopia.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  internal class CryptopiaExchange : Exchange
  {
    CryptopiaApiPublic publicApi;

    public CryptopiaExchange()
    {
      publicApi = new CryptopiaApiPublic();
    }

    public override async Task<List<TradingPair>> GetAllTradingPairs()
    {
      MarketsResponse response = await publicApi.GetMarkets(new MarketsRequest());
      if(response.Success == false)
      {
        return null;
      }

      List<TradingPair> tradingPairList = new List<TradingPair>();

      for (int i = 0; i < response.Data.Count; i++)
      {
        MarketResult result = response.Data[i];
        tradingPairList.Add(result.ToTradingPair());
      }

      return tradingPairList;
    }
  }
}
