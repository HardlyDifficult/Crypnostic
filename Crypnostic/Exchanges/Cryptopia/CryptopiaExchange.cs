using System;
using Cryptopia.API;
using Cryptopia.API.DataObjects;
using Cryptopia.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD;
using System.Diagnostics;

namespace Crypnostic
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
      CrypnosticController exchangeMonitor)
      : base(exchangeMonitor, ExchangeName.Cryptopia, 1_000_000 / 1_440)
    {
      publicApi = new CryptopiaApiPublic();
      AddBlacklistedTicker(
        "Trinity" // This is not TNC,
        , "Trinity Network Credit" // TODO this is here since another exchange may have aliased Trinity
        );

      exchangeMonitor.AddAlias("MyWishToken", "MyWish");
      exchangeMonitor.AddAlias("BCash", "Bitcoin Cash");
      exchangeMonitor.AddAlias("KyberNetworkCrystal", "Kyber Network");
      exchangeMonitor.AddAlias("NewEconomyMovement", "NEM");
      exchangeMonitor.AddAlias("OysterPearl", "Oyster");
      exchangeMonitor.AddAlias("Tronix", "TRON");
      exchangeMonitor.AddAlias("BlockMason", "BlockMason Credit Protocol");
      exchangeMonitor.AddAlias("GeneChain", "EncrypGen");
      exchangeMonitor.AddAlias("Iquant Chain", "iQuant");
      exchangeMonitor.AddAlias("NavCoin", "NAV Coin");
      exchangeMonitor.AddAlias("WildCrypto", "Wild Crypto");
      exchangeMonitor.AddAlias("1337", "Elite");
      exchangeMonitor.AddAlias("808", "808Coin");
      exchangeMonitor.AddAlias("ArgusCoin", "Argus");
      exchangeMonitor.AddAlias("Atomiccoin", "Atomic Coin");
      exchangeMonitor.AddAlias("ChronosCoin", "Chronos");
      exchangeMonitor.AddAlias("ClamCoin", "Clams");
      exchangeMonitor.AddAlias("Coimatic 3", "Coimatic 3.0");
      exchangeMonitor.AddAlias("Coin2", "Coin2.1");
      exchangeMonitor.AddAlias("CometCoin", "Comet");
      exchangeMonitor.AddAlias("CREAMcoin", "Cream");
      exchangeMonitor.AddAlias("CryptoBullion", "Crypto Bullion");
      exchangeMonitor.AddAlias("CryptopiaFeeShare", "CryptopiaFeeShares");
      exchangeMonitor.AddAlias("DDF", "DigitalDevelopersFund");
      exchangeMonitor.AddAlias("Divi Exchange Token", "Divi");
      exchangeMonitor.AddAlias("EncryptoTel", "EncryptoTel [ETH]");
      exchangeMonitor.AddAlias("EquiTrade", "EquiTrader");
      exchangeMonitor.AddAlias("Evilcoin", "Evil Coin");
      exchangeMonitor.AddAlias("FootyCash", "Footy Cash");
      exchangeMonitor.AddAlias("GaiaCoin", "GAIA");
      exchangeMonitor.AddAlias("GayMoney", "GAY Money");
      exchangeMonitor.AddAlias("GPUCoin", "GPU Coin");
      exchangeMonitor.AddAlias("Growers Intl", "Growers International");
      exchangeMonitor.AddAlias("Hackspace", "Hackspace Capital");
      exchangeMonitor.AddAlias("HarvestCoin", "Harvest Masternode Coin");
      exchangeMonitor.AddAlias("HeatLedger", "HEAT");
      exchangeMonitor.AddAlias("HexxCoin", "Hexx");
      exchangeMonitor.AddAlias("HodlBucks", "HODL Bucks");
      exchangeMonitor.AddAlias("IgnitionCoin", "Ignition");
      exchangeMonitor.AddAlias("Insane", "InsaneCoin");
      exchangeMonitor.AddAlias("InterstellarHoldings", "Interstellar Holdings");
      exchangeMonitor.AddAlias("Kubera", "Kubera Coin");
      exchangeMonitor.AddAlias("LindaCoin", "Linda");
      exchangeMonitor.AddAlias("litecoinPlus", "Litecoin Plus");
      exchangeMonitor.AddAlias("LitecoinUltra", "LiteCoin Ultra");
      exchangeMonitor.AddAlias("Mars", "Marscoin");
      exchangeMonitor.AddAlias("MazaCoin", "MAZA");
      exchangeMonitor.AddAlias("MonkeyProject", "Monkey Project");
      exchangeMonitor.AddAlias("MyBit", "MyBit Token");
      exchangeMonitor.AddAlias("Opalcoin", "Opal");
      exchangeMonitor.AddAlias("OrmeusCoin", "Ormeus Coin");
      exchangeMonitor.AddAlias("PascalLite", "Pascal Lite");
      exchangeMonitor.AddAlias("PhilosopherStone", "Philosopher Stones");
      exchangeMonitor.AddAlias("PioneerCoin", "Pioneer Coin");
      exchangeMonitor.AddAlias("PoSWallet", "PoSW Coin");
      exchangeMonitor.AddAlias("RenosCoin", "Renos");
      exchangeMonitor.AddAlias("RoyalKingdomCoin", "Royal Kingdom Coin");
      exchangeMonitor.AddAlias("SBC Coin", "StrikeBitClub");
      exchangeMonitor.AddAlias("SHACoin2", "SHACoin");
      exchangeMonitor.AddAlias("SocialSend", "Social Send");
      exchangeMonitor.AddAlias("Steneum", "Steneum Coin");
      exchangeMonitor.AddAlias("Swingcoin", "Swing");
      exchangeMonitor.AddAlias("Synereo AMP", "Synereo");
      exchangeMonitor.AddAlias("TokugawaCoin", "Tokugawa");
      exchangeMonitor.AddAlias("UniversalCurrency", "Universal Currency");
      exchangeMonitor.AddAlias("Verium", "VeriumReserve");
    }

    public override async Task LoadTickerNames()
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
        AddTicker(product.Symbol, CreateFromName(product.Name), isCoinActive);
      }

      await throttle.WaitTillReady();
      TradePairsResponse tradePairsResponse = await publicApi.GetTradePairs();
      for (int i = 0; i < tradePairsResponse.Data.Count; i++)
      {
        TradePairResult tradePair = tradePairsResponse.Data[i];
        (Coin, Coin) entry = (CreateFromName(tradePair.Currency),
          CreateFromName(tradePair.BaseCurrency));
        entry.Item1?.UpdatePairStatus(this, entry.Item2, tradePair.Status != "OK");
      }
    }

    protected override async Task GetAllTradingPairs()
    {
      const string tradingPairSeparator = "/";

      await throttle.WaitTillReady();
      MarketsResponse tickerList = await publicApi.GetMarkets(new MarketsRequest());
      foreach (MarketResult ticker in tickerList.Data)
      {
        TradingPair pair = AddTradingPair(baseCoinTicker: ticker.Label.GetAfter(tradingPairSeparator),
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
    
    protected override async Task<OrderBook> GetOrderBookInternal(
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
