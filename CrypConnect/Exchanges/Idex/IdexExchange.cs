using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrypConnect.Exchanges.Idex;
using CrypConnect.Exchanges.Kucoin;
using RestSharp;
using HD;

namespace CrypConnect.Exchanges
{
  /// <remarks>
  /// https://github.com/AuroraDAO/idex-api-docs
  /// </remarks>
  internal class IdexExchange : RestExchange
  {
    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    public IdexExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Idex, 1_000_000 / 1_440,
          "https://api.idex.market", Method.POST)
    {
      AddBlacklistedTicker(
        "SGT", // Creates dupe coin name. This one has no volume
        "INDIOLD", // Replaced by "Indi"
        "PPT2", // Creates dupe coin name. This one has no volume
        "DVIP" // Blocked from the Idex front end
        );
      exchangeMonitor.AddAlias("Ether", "Ethereum");
      exchangeMonitor.AddAlias("OMGToken", "OmiseGO");
    }

    public override async Task LoadTickerNames()
    {
      Dictionary<string, IdexTickerInfoJson> productList = await Get<Dictionary<string, IdexTickerInfoJson>>(
        "returnCurrencies");

      foreach (KeyValuePair<string, IdexTickerInfoJson> product
        in productList)
      {
        string ticker = product.Key;
        string fullName = product.Value.name;
        Coin coin = Coin.CreateFromName(fullName);
        bool isInactive = false;
        AddTicker(ticker, coin, isInactive);
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      Dictionary<string, IdexReturnTickerJson> tickerList = await Get<Dictionary<string, IdexReturnTickerJson>>(
        "returnTicker");

      foreach (KeyValuePair<string, IdexReturnTickerJson> ticker in tickerList)
      {
        string baseCoinTicker = ticker.Key.GetBefore("_");
        string quoteCoinTicker = ticker.Key.GetAfter("_");
        decimal askPrice = Parse(ticker.Value.lowestAsk);
        decimal bidPrice = Parse(ticker.Value.highestBid);
        bool isInactive = false;

        TradingPair pair = AddTradingPair(baseCoinTicker, quoteCoinTicker, askPrice, bidPrice, isInactive);

        if (pair != null)
        {
          // TODO Unknown last volume without a follow up request
          pair.lastTrade = new LastTrade(Parse(ticker.Value.last), 0); 
        }
      }
    }

    static decimal Parse(
      string value)
    {
      if (value == null || value.Equals("N/A", StringComparison.InvariantCultureIgnoreCase))
      {
        return 0;
      }
      else
      {
        return decimal.Parse(value);
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}_{baseSymbol.ToUpperInvariant()}";
    }
  }
}
