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
      exchangeMonitor.AddAlias("Digix DAO", "DigixDAO");
      exchangeMonitor.AddAlias("Kyber", "Kyber Network");
      exchangeMonitor.AddAlias("Poet", "Po.et");
      exchangeMonitor.AddAlias("Raiden", "Raiden Network Token");
      exchangeMonitor.AddAlias("Ripio", "Ripio Credit Network");
      exchangeMonitor.AddAlias("Singularity.net", "SingularityNET");
      exchangeMonitor.AddAlias("SALT Lending", "SALT");
      exchangeMonitor.AddAlias("StorjToken", "Storj");
      exchangeMonitor.AddAlias("BNB", "Binance Coin");
      exchangeMonitor.AddAlias("Blockarray", "Block Array");
      exchangeMonitor.AddAlias("Decent.Bet", "DecentBet");
      exchangeMonitor.AddAlias("Flixx", "Flixxo");
      exchangeMonitor.AddAlias("Gene-Chain Coin", "EncrypGen");
      exchangeMonitor.AddAlias("Hawala Today", "Hawala.Today");
      exchangeMonitor.AddAlias("iExec", "iExec RLC");
      exchangeMonitor.AddAlias("IOS", "IOStoken");
      exchangeMonitor.AddAlias("IQT", "iQuant");
      exchangeMonitor.AddAlias("ITT", "Intelligent Trading Tech");
      exchangeMonitor.AddAlias("Moeda", "Moeda Loyalty Points");
      exchangeMonitor.AddAlias("Ormeus", "OrmeusCoin");
      exchangeMonitor.AddAlias("QRL", "Quantum Resistant Ledger");
      exchangeMonitor.AddAlias("RHOC", "RChain");
      exchangeMonitor.AddAlias("RvT", "Rivetz");
      exchangeMonitor.AddAlias("SIRIN LABS", "SIRIN LABS Token");
      exchangeMonitor.AddAlias("Trade.io", "Trade Token");
      exchangeMonitor.AddAlias("WePower Token", "WePower");
      exchangeMonitor.AddAlias("WILD", "Wild Crypto");
      exchangeMonitor.AddAlias("Accelerator", "Accelerator Network");
      exchangeMonitor.AddAlias("AstroTokens", "Astro");
      exchangeMonitor.AddAlias("Cloudwith.me", "Cloud");
      exchangeMonitor.AddAlias("Cofoundit", "Cofound.it");
      exchangeMonitor.AddAlias("Crystal", "Crystal Clear");
      exchangeMonitor.AddAlias("DaoCasino", "DAO.Casino");
      exchangeMonitor.AddAlias("DataWallet Token", "Datawallet");
      exchangeMonitor.AddAlias("EthereumMovieVenture", "Ethereum Movie Venture");
      exchangeMonitor.AddAlias("Fusion Token", "Fusion");
      exchangeMonitor.AddAlias("Hubiits", "Hubii Network");
      exchangeMonitor.AddAlias("HYPERTV", "Hyper TV");
      exchangeMonitor.AddAlias("ICOS - ICOBox", "ICOS");
      exchangeMonitor.AddAlias("INDAHASH COIN", "indaHash");
      exchangeMonitor.AddAlias("Indorse", "Indorse Token");
      exchangeMonitor.AddAlias("Jibrel Network Token", "Jibrel Network");
      exchangeMonitor.AddAlias("LIFE Token", "LIFE");
      exchangeMonitor.AddAlias("Link", "Link Platform");
      exchangeMonitor.AddAlias("MedToken", "Medicalchain");
      exchangeMonitor.AddAlias("Melonport", "Melon");
      exchangeMonitor.AddAlias("NeuroToken", "Neuro");
      exchangeMonitor.AddAlias("Origin Trail", "OriginTrail");
      exchangeMonitor.AddAlias("PlayKey Token", "Playkey");
      exchangeMonitor.AddAlias("Real.Markets", "REAL");
      exchangeMonitor.AddAlias("SANtiment", "Santiment Network Token");
      exchangeMonitor.AddAlias("Storm Market", "Storm");
      exchangeMonitor.AddAlias("Streamr", "Streamr DATAcoin");
      exchangeMonitor.AddAlias("Target", "Target Coin");
      exchangeMonitor.AddAlias("Theta", "Theta Token");
      exchangeMonitor.AddAlias("Tie Token", "TIES Network");
      exchangeMonitor.AddAlias("Wi", "Wi Coin");
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
