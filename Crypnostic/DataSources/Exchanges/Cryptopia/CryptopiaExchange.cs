using System;
using Cryptopia.API;
using Cryptopia.API.DataObjects;
using Cryptopia.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;

namespace Crypnostic.Internal
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
    public CryptopiaExchange()
      : base(ExchangeName.Cryptopia, 1_000_000 / 1_440)
    {
      publicApi = new CryptopiaApiPublic();
      AddBlacklistedTicker(
        "Trinity" // This is not TNC
        );

      CrypnosticController.instance.AddCoinAlias(
        new[] { "MyWish", "MyWishToken" },
        new[] { "Bitcoin Cash", "BCash" },
        new[] { "Kyber Network", "KyberNetworkCrystal" },
        new[] { "NEM", "NewEconomyMovement" },
        new[] { "Oyster", "OysterPearl" },
        new[] { "TRON", "Tronix" },
        new[] { "BlockMason Credit Protocol", "BlockMason" },
        new[] { "EncrypGen", "GeneChain" },
        new[] { "iQuant", "Iquant Chain" },
        new[] { "NAV Coin", "NavCoin" },
        new[] { "Wild Crypto", "WildCrypto" },
        new[] { "Elite", "1337" },
        new[] { "808Coin", "808" },
        new[] { "Argus", "ArgusCoin" },
        new[] { "Atomic Coin", "Atomiccoin" },
        new[] { "Chronos", "ChronosCoin" },
        new[] { "Clams", "ClamCoin" },
        new[] { "Coimatic 3.0", "Coimatic 3" },
        new[] { "Coin2.1", "Coin2" },
        new[] { "Comet", "CometCoin" },
        new[] { "Cream", "CREAMcoin" },
        new[] { "Crypto Bullion", "CryptoBullion" },
        new[] { "CryptopiaFeeShares", "CryptopiaFeeShare" },
        new[] { "DigitalDevelopersFund", "DDF" },
        new[] { "Divi", "Divi Exchange Token" },
        new[] { "EncryptoTel [ETH]", "EncryptoTel" },
        new[] { "EquiTrader", "EquiTrade" },
        new[] { "Evil Coin", "Evilcoin" },
        new[] { "Footy Cash", "FootyCash" },
        new[] { "GAIA", "GaiaCoin" },
        new[] { "GAY Money", "GayMoney" },
        new[] { "GPU Coin", "GPUCoin" },
        new[] { "Growers International", "Growers Intl" },
        new[] { "Hackspace Capital", "Hackspace" },
        new[] { "Harvest Masternode Coin", "HarvestCoin" },
        new[] { "HEAT", "HeatLedger" },
        new[] { "Hexx", "HexxCoin" },
        new[] { "HODL Bucks", "HodlBucks" },
        new[] { "Ignition", "IgnitionCoin" },
        new[] { "InsaneCoin", "Insane" },
        new[] { "Interstellar Holdings", "InterstellarHoldings" },
        new[] { "Kubera Coin", "Kubera" },
        new[] { "Linda", "LindaCoin" },
        new[] { "Litecoin Plus", "litecoinPlus" },
        new[] { "LiteCoin Ultra", "LitecoinUltra" },
        new[] { "Marscoin", "Mars" },
        new[] { "MAZA", "MazaCoin" },
        new[] { "Monkey Project", "MonkeyProject" },
        new[] { "MyBit Token", "MyBit" },
        new[] { "Opal", "Opalcoin" },
        new[] { "Ormeus Coin", "OrmeusCoin" },
        new[] { "Pascal Lite", "PascalLite" },
        new[] { "Philosopher Stones", "PhilosopherStone" },
        new[] { "Pioneer Coin", "PioneerCoin" },
        new[] { "PoSW Coin", "PoSWallet" },
        new[] { "Renos", "RenosCoin" },
        new[] { "Royal Kingdom Coin", "RoyalKingdomCoin" },
        new[] { "StrikeBitClub", "SBC Coin" },
        new[] { "SHACoin", "SHACoin2" },
        new[] { "Social Send", "SocialSend" },
        new[] { "Steneum Coin", "Steneum" },
        new[] { "Swing", "Swingcoin" },
        new[] { "Synereo", "Synereo AMP" },
        new[] { "Tokugawa", "TokugawaCoin" },
        new[] { "Universal Currency", "UniversalCurrency" },
        new[] { "VeriumReserve", "Verium" });
    }

    protected override async Task RefreshTickers()
    {
      await throttle.WaitTillReady();
      CurrenciesResponse currenciesResponse = await publicApi.GetCurrencies();
      for (int i = 0; i < currenciesResponse.Data.Count; i++)
      {
        CurrencyResult product = currenciesResponse.Data[i];
        if (product.ListingStatus.Equals("Active",
          StringComparison.InvariantCultureIgnoreCase) == false)
        { // De-listed coins should not be considered at all..
          continue;
        }
        bool isCoinActive = true;
        if (product.Status != "OK")
        {
          isCoinActive = false;
        }
        await AddTicker(await CreateFromName(product.Name), product.Symbol, isCoinActive);
      }

      await throttle.WaitTillReady();
      TradePairsResponse tradePairsResponse = await publicApi.GetTradePairs();
      for (int i = 0; i < tradePairsResponse.Data.Count; i++)
      {
        TradePairResult tradePair = tradePairsResponse.Data[i];
        (Coin, Coin) entry = (await CreateFromName(tradePair.Currency),
          await CreateFromName(tradePair.BaseCurrency));
        entry.Item1?.UpdatePairStatus(this, entry.Item2, tradePair.Status != "OK");
      }
    }

    protected override async Task RefreshTradingPairs()
    {
      const string tradingPairSeparator = "/";

      await throttle.WaitTillReady();
      MarketsResponse tickerList = await publicApi.GetMarkets(new MarketsRequest());
      foreach (MarketResult ticker in tickerList.Data)
      {
        TradingPair pair = await AddTradingPair(baseCoinTicker: ticker.Label.GetAfter(tradingPairSeparator),
          quoteCoinTicker: ticker.Label.GetBefore(tradingPairSeparator),
          askPrice: ticker.AskPrice,
          bidPrice: ticker.BidPrice);
        if (pair != null)
        {
          pair.lastTrade = new LastTrade(ticker.LastPrice);
        }
      }
    }

    protected override string GetPairId(
      string quoteSymbol,
      string baseSymbol)
    {
      return $"{quoteSymbol.ToUpperInvariant()}_{baseSymbol.ToUpperInvariant()}";
    }

    protected override async Task<OrderBook> GetOrderBook(
      string pairId)
    {
      MarketOrdersRequest ordersRequest = new MarketOrdersRequest(pairId);
      MarketOrdersResponse ordersResponse = await publicApi.GetMarketOrders(ordersRequest);

      Order[] bids = ExtractOrders(ordersResponse.Data.Buy);
      Order[] asks = ExtractOrders(ordersResponse.Data.Sell);

      return new OrderBook(asks, bids);
    }

    static Order[] ExtractOrders(
      List<MarketOrderResult> resultList)
    {
      Order[] orderList = new Order[resultList.Count];
      for (int i = 0; i < orderList.Length; i++)
      {
        MarketOrderResult orderResult = resultList[i];
        orderList[i] = new Order(orderResult.Price, orderResult.Volume);
      }

      return orderList;
    }
  }
}
