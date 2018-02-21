using System;
using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Market;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;

namespace Crypnostic.Internal
{
  /// <summary>
  /// </summary>
  /// <remarks>
  /// https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md
  /// HTTP 429 return code is used when breaking a request rate limit.
  /// </remarks>
  internal class BinanceExchange : RestExchange
  {
    // TODO remove this dependancy
    readonly BinanceClient client;

    /// <summary>
    /// Throttle:
    ///   1200 requests per minute is the stated max.
    ///   Targeting half that to avoid issues.
    /// </summary>
    public BinanceExchange()
      : base(ExchangeName.Binance,
          "https://www.binance.com",
          1200)
    {
      CrypnosticController.instance.AddCoinAlias(
        new[] { "Tether", "TetherUS" },
        new[] { "Kyber Network", "KyberNetwork" },
        new[] { "Enjin Coin", "EnjinCoin" },
        new[] { "iExec RLC", "iExecRLC" },
        new[] { "IOTA", "MIOTA" },
        new[] { "Gas", "NeoGas" },
        new[] { "Power Ledger", "PowerLedger" },
        new[] { "Stellar", "Stellar Lumens" },
        new[] { "Waltonchain", "Walton" },
        new[] { "AmberCoin", "Amber" },
        new[] { "ChatCoin", "CHAT" });

      ApiClient api = new ApiClient(null, null);
      client = new BinanceClient(api);
    }

    protected override async Task RefreshTickers()
    {
      BinanceProductListJson productList =
        await Get<BinanceProductListJson>("exchange/public/product");
      if (productList == null)
      {
        return;
      }

      for (int i = 0; i < productList.data.Length; i++)
      {
        BinanceProductJson product = productList.data[i];
        bool isInactive = product.status.Equals("TRADING",
          StringComparison.InvariantCultureIgnoreCase) == false;
        Coin baseCoin = await CreateFromName(product.baseAssetName);
        // TODO Binance: how-to determine the coin's status (e.g. deposit paused)
        await AddTicker(baseCoin, product.baseAsset, false);
        Coin quoteCoin = await CreateFromName(product.quoteAssetName);
        await AddTicker(quoteCoin, product.quoteAsset, false);
        baseCoin?.UpdatePairStatus(this, quoteCoin, isInactive);
      }
    }

    protected override async Task RefreshTradingPairs()
    {
      await throttle.WaitTillReady();
      IEnumerable<OrderBookTicker> tickerList = await client.GetOrderBookTicker();

      foreach (OrderBookTicker ticker in tickerList)
      {
        string baseCoinTicker = null;
        string symbolLower = ticker.Symbol.ToLowerInvariant();
        foreach (KeyValuePair<string, Coin> tickerToName in tickerLowerToCoin)
        {
          if (symbolLower.EndsWith(tickerToName.Key))
          {
            baseCoinTicker = tickerToName.Key;
          }
        }

        string quoteCoinTicker = ticker.Symbol.Substring(0, ticker.Symbol.Length - baseCoinTicker?.Length ?? 0);

        await AddTradingPair(baseCoinTicker: baseCoinTicker,
          quoteCoinTicker: quoteCoinTicker,
          askPrice: ticker.AskPrice,
          bidPrice: ticker.BidPrice);
      }
    }

    internal override async Task RefreshLastTrade(
      TradingPair tradingPair)
    {
      // https://www.binance.com/api/v1/trades?symbol=ETHBTC&limit=1
      List<BinanceTradeJson> tradeHistory = await Get<List<BinanceTradeJson>>(
        $"api/v1/trades?symbol={GetPairId(tradingPair)}&limit=1");
      if (tradeHistory == null || tradeHistory.Count == 0)
      {
        return;
      }

      BinanceTradeJson history = tradeHistory[0];
      decimal price = decimal.Parse(history.price);
      tradingPair.lastTrade = new LastTrade(price);
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}{baseSymbol.ToUpperInvariant()}";
    }

    protected override async Task UpdateOrderBook(
     string pairId,
     OrderBook orderBook)
    {
      BinanceDepthJson depthJson = await Get<BinanceDepthJson>($"api/v1/depth?symbol={pairId}");
      if(depthJson == null)
      {
        return;
      }

      Order[] bids = ExtractOrders(depthJson.bids);
      Order[] asks = ExtractOrders(depthJson.asks);

      orderBook.Update(asks, bids);
    }

    static Order[] ExtractOrders(
      object[][] resultList)
    {
      Order[] orderList = new Order[resultList.Length];
      for (int i = 0; i < orderList.Length; i++)
      {
        orderList[i] = new Order(decimal.Parse((string)resultList[i][0]),
          decimal.Parse((string)resultList[i][1]));
      }

      return orderList;
    }
  }
}
