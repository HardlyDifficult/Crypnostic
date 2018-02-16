﻿using System;
using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Market;
using CrypConnect.Exchanges.Binance;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;
using CrypConnect.Exchanges;

namespace CrypConnect
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
    public BinanceExchange(
      ExchangeMonitor exchangeMonitor)
      : base(exchangeMonitor,
          ExchangeName.Binance,
          1200,
          "https://www.binance.com")
    {
      exchangeMonitor.AddAlias("TetherUS", "Tether");
      exchangeMonitor.AddAlias("KyberNetwork", "Kyber Network");
      exchangeMonitor.AddAlias("EnjinCoin", "Enjin Coin");
      exchangeMonitor.AddAlias("iExecRLC", "iExec RLC");
      exchangeMonitor.AddAlias("MIOTA", "IOTA");
      exchangeMonitor.AddAlias("NeoGas", "Gas");
      exchangeMonitor.AddAlias("PowerLedger", "Power Ledger");
      exchangeMonitor.AddAlias("Stellar Lumens", "Stellar");
      exchangeMonitor.AddAlias("Walton", "Waltonchain");
      exchangeMonitor.AddAlias("Amber", "AmberCoin");
      exchangeMonitor.AddAlias("CHAT", "ChatCoin");

      ApiClient api = new ApiClient(null, null);
      client = new BinanceClient(api);
    }

    public override async Task LoadTickerNames()
    {
      BinanceProductListJson productList =
        await Get<BinanceProductListJson>("exchange/public/product");

      for (int i = 0; i < productList.data.Length; i++)
      {
        BinanceProductJson product = productList.data[i];
        bool isInactive = product.status.Equals("TRADING",
          StringComparison.InvariantCultureIgnoreCase) == false;
        Coin baseCoin = Coin.CreateFromName(product.baseAssetName);
        // TODO Binance: how-to determine the coin's status (e.g. deposit paused)
        AddTicker(product.baseAsset, baseCoin, false);
        Coin quoteCoin = Coin.CreateFromName(product.quoteAssetName);
        AddTicker(product.quoteAsset, quoteCoin, false);
        baseCoin?.UpdatePairStatus(this, quoteCoin, isInactive);
      }
    }

    protected override async Task GetAllTradingPairs()
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

        AddTradingPair(baseCoinTicker: baseCoinTicker,
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
      if (tradeHistory.Count > 0)
      {
        BinanceTradeJson history = tradeHistory[0];
        decimal price = decimal.Parse(history.price);
        decimal volume = decimal.Parse(history.qty);
        tradingPair.lastTrade = new LastTrade(price, volume);
      }
    }

    protected override string GetPairId(
      string quoteSymbol, 
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}{baseSymbol.ToUpperInvariant()}";
    }
  }
}
