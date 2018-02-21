using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using HD;

namespace Crypnostic
{
  /// <remarks>
  /// https://github.com/AuroraDAO/idex-api-docs
  /// </remarks>
  internal class IdexExchange : RestExchange
  {
    public override bool supportsOverlappingBooks
    {
      get
      { // Not common, but does happen
        return true;
      }
    }

    /// <summary>
    /// No stated throttle limit, going with the same as Crytpopia
    /// </summary>
    public IdexExchange()
      : base(ExchangeName.Idex, "https://api.idex.market", 1_000_000 / 1_440,
          method: Method.POST)
    {
      AddBlacklistedTicker(
        "SGT", // Creates dupe coin name. This one has no volume
        "INDIOLD", // Replaced by "Indi"
        "PPT2", // Creates dupe coin name. This one has no volume
        "DVIP" // Blocked from the Idex front end
        );

      CrypnosticController.instance.AddCoinAlias(
        new[] { "Ethereum", "Ether" },
        new[] { "OmiseGO", "OMGToken" },
        new[] { "DigixDAO", "Digix DAO" },
        new[] { "Kyber Network", "Kyber" },
        new[] { "Po.et", "Poet" },
        new[] { "Raiden Network Token", "Raiden" },
        new[] { "Ripio Credit Network", "Ripio" },
        new[] { "SingularityNET", "Singularity.net" },
        new[] { "SALT", "SALT Lending" },
        new[] { "Storj", "StorjToken" },
        new[] { "Binance Coin", "BNB" },
        new[] { "Block Array", "Blockarray" },
        new[] { "DecentBet", "Decent.Bet" },
        new[] { "Flixxo", "Flixx" },
        new[] { "EncrypGen", "Gene-Chain Coin" },
        new[] { "Hawala.Today", "Hawala Today" },
        new[] { "iExec RLC", "iExec" },
        new[] { "IOStoken", "IOS" },
        new[] { "iQuant", "IQT" },
        new[] { "Intelligent Trading Tech", "ITT" },
        new[] { "Moeda Loyalty Points", "Moeda" },
        new[] { "OrmeusCoin", "Ormeus" },
        new[] { "Quantum Resistant Ledger", "QRL" },
        new[] { "RChain", "RHOC" },
        new[] { "Rivetz", "RvT" },
        new[] { "SIRIN LABS Token", "SIRIN LABS" },
        new[] { "Trade Token", "Trade.io" },
        new[] { "WePower", "WePower Token" },
        new[] { "Wild Crypto", "WILD" },
        new[] { "Accelerator Network", "Accelerator" },
        new[] { "Astro", "AstroTokens" },
        new[] { "Cloud", "Cloudwith.me" },
        new[] { "Cofound.it", "Cofoundit" },
        new[] { "Crystal Clear", "Crystal" },
        new[] { "DAO.Casino", "DaoCasino" },
        new[] { "Datawallet", "DataWallet Token" },
        new[] { "Ethereum Movie Venture", "EthereumMovieVenture" },
        new[] { "Fusion", "Fusion Token" },
        new[] { "Hubii Network", "Hubiits" },
        new[] { "Hyper TV", "HYPERTV" },
        new[] { "ICOS", "ICOS - ICOBox" },
        new[] { "indaHash", "INDAHASH COIN" },
        new[] { "Indorse Token", "Indorse" },
        new[] { "Jibrel Network", "Jibrel Network Token" },
        new[] { "LIFE", "LIFE Token" },
        new[] { "Link Platform", "Link" },
        new[] { "Medicalchain", "MedToken" },
        new[] { "Melon", "Melonport" },
        new[] { "Neuro", "NeuroToken" },
        new[] { "OriginTrail", "Origin Trail" },
        new[] { "Playkey", "PlayKey Token" },
        new[] { "REAL", "Real.Markets" },
        new[] { "Santiment Network Token", "SANtiment" },
        new[] { "Storm", "Storm Market" },
        new[] { "Streamr DATAcoin", "Streamr" },
        new[] { "Target Coin", "Target" },
        new[] { "Theta Token", "Theta" },
        new[] { "TIES Network", "Tie Token" },
        new[] { "Wi Coin", "Wi" });
    }

    protected override async Task RefreshTickers()
    {
      Dictionary<string, IdexTickerInfoJson> productList = await Get<Dictionary<string, IdexTickerInfoJson>>(
        "returnCurrencies");
      if(productList == null)
      {
        return;
      }

      foreach (KeyValuePair<string, IdexTickerInfoJson> product
        in productList)
      {
        string ticker = product.Key;
        string fullName = product.Value.name;
        Coin coin = await CreateFromName(fullName);
        bool isInactive = false;
        await AddTicker(coin, ticker, isInactive);
      }
    }

    protected override async Task RefreshTradingPairs()
    {
      Dictionary<string, IdexReturnTickerJson> tickerList = await Get<Dictionary<string, IdexReturnTickerJson>>(
        "returnTicker");
      if(tickerList == null)
      {
        return;
      }

      foreach (KeyValuePair<string, IdexReturnTickerJson> ticker in tickerList)
      {
        string baseCoinTicker = ticker.Key.GetBefore("_");
        string quoteCoinTicker = ticker.Key.GetAfter("_");
        decimal askPrice = Parse(ticker.Value.lowestAsk);
        decimal bidPrice = Parse(ticker.Value.highestBid);
        bool isInactive = false;

        TradingPair pair = await AddTradingPair(baseCoinTicker, quoteCoinTicker, askPrice, bidPrice, isInactive);

        if (pair != null)
        {
          pair.lastTrade = new LastTrade(Parse(ticker.Value.last));
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
      return $"{baseSymbol.ToUpperInvariant()}_{quoteSymbol.ToUpperInvariant()}";
    }

    protected override async Task<OrderBook> GetOrderBook(
     string pairId)
    {
      IdexRequestForMarket request = new IdexRequestForMarket();
      request.market = pairId;
      IdexDepthListJson depthJson = await Get<IdexDepthListJson>(
        $"returnOrderBook", request);
      if(depthJson == null)
      {
        return default(OrderBook);
      }

      Order[] bids = ExtractOrders(depthJson.Bids);
      Order[] asks = ExtractOrders(depthJson.Asks);

      return new OrderBook(asks, bids);
    }

    static Order[] ExtractOrders(
      IdexDepthJson[] resultList)
    {
      Order[] orderList = new Order[resultList?.Length ?? 0];
      for (int i = 0; i < orderList.Length; i++)
      {
        orderList[i] = new Order(decimal.Parse(resultList[i].Price),
          decimal.Parse(resultList[i].Amount));
      }

      return orderList;
    }
  }
}
