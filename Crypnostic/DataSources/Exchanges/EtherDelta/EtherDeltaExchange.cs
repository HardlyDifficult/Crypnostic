using System;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;

namespace Crypnostic.Internal
{
  /// <summary>
  /// TODO EtherDelta lastTrade
  /// </summary>
  /// <remarks>
  /// https://github.com/etherdelta/etherdelta.github.io/blob/master/docs/API_OLD.md
  /// </remarks>
  internal class EtherDeltaExchange : RestExchange
  {
    public override bool supportsOverlappingBooks
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    public EtherDeltaExchange()
      : base(ExchangeName.EtherDelta, "https://api.forkdelta.com", 5)
    {
      AddBlacklistedTicker(
        "0x75c79b88f" // not NEO's Gas
        );
    }

    protected override async Task RefreshTickers()
    {
      EtherDeltaMainConfigJson json = await Get<EtherDeltaMainConfigJson>(
        "https://forkdelta.github.io/config/main.json");
      for (int i = 0; i < json.tokens.Length; i++)
      {
        Token token = json.tokens[i];
        Coin coin = Coin.FromTicker(token.name);
        if (coin == null)
        { // If coinmarketcap does not have the ticker, I don't know what to do
          continue;
        }
        await AddTicker(coin, token.addr.Substring(0, 9), false);
      }
    }

    protected override async Task RefreshTradingPairs()
    {
      Dictionary<string, Dictionary<string, object>> tickerList;
      tickerList =
        await Get<Dictionary<string, Dictionary<string, object>>>("returnTicker");

      if (tickerList == null)
      {
        return;
      }

      foreach (var ticker in tickerList)
      {
        string askString = ticker.Value["ask"]?.ToString();
        if (string.IsNullOrWhiteSpace(askString))
        {
          askString = "0";
        }
        string bidString = ticker.Value["bid"]?.ToString();
        if (string.IsNullOrWhiteSpace(bidString))
        {
          bidString = "0";
        }

        try
        {
          decimal.TryParse(askString, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out decimal ask);
          decimal.TryParse(bidString, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out decimal bid);


          await AddTradingPair(
            ticker.Key.GetAfter("_"),
            Coin.ethereum,
            ask,
            bid,
            false);
        }
        catch (Exception e)
        {
          Console.WriteLine();
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}-{baseSymbol.ToUpperInvariant()}";
    }

    protected override Task UpdateOrderBook(
      string pairId,
      OrderBook orderBook)
    {
      throw new NotImplementedException();
    }
  }
}
