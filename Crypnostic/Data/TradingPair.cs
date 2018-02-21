using Common.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Crypnostic
{
  /// <summary>
  /// This represents a coin+coin pair on a specific exchange.
  /// The values are periodically updated automatically.
  /// </summary>
  public class TradingPair
  {
    #region Public Data
    public event Action onPriceUpdate;

    /// <summary>
    /// Called if this pair's active status changes.
    /// e.g. the pair comes out of maintenance mode.
    /// </summary>
    public event Action onStatusChange;

    public readonly Exchange exchange;

    public readonly Coin baseCoin;

    public readonly Coin quoteCoin;
    
    public decimal askPriceOrOfferYouCanBuy
    {
      get; private set;
    }

    public decimal bidPriceOrOfferYouCanSell
    {
      get; private set;
    }

    public DateTime lastUpdated
    {
      get; private set;
    }

    public readonly OrderBook orderBook;
    #endregion

    #region Internal Data
    static readonly ILog log = LogManager.GetLogger<TradingPair>();

    /// <summary>
    /// Use the GetLastTrade to allow for async and caching.
    /// </summary>
    internal LastTrade lastTrade
    {
      private get; set;
    }


    bool _isInactive;
    #endregion

    #region Public Properties
    /// <summary>
    /// A pair is inactive if the exchange reported it that way
    /// or if there are no valid offers found.
    /// </summary>
    public bool isInactive
    {
      get
      {
        return _isInactive || askPriceOrOfferYouCanBuy == 0 && bidPriceOrOfferYouCanSell == 0;
      }
      set
      {
        //Debug.Assert(baseCoin != Coin.litecoin || value); // This may not hold 

        if (_isInactive == value)
        { // No change
          return;
        }

        _isInactive = value;
        onStatusChange?.Invoke();
      }
    }
    #endregion

    #region Init
    /// <summary>
    /// Note that pairs are unique, i.e. use Update instead
    /// of re-creating Pairs.
    /// </summary>
    internal TradingPair(
      Exchange exchange,
      Coin baseCoin,
      Coin quoteCoin,
      decimal askPriceOrOfferYouCanBuy,
      decimal bidPriceOrOfferYouCanSell,
      bool isInactive = false)
    {
      log.Trace($"Creating a pair for {quoteCoin}-{baseCoin}");

      Debug.Assert(exchange != null);
      Debug.Assert(baseCoin != null);
      Debug.Assert(quoteCoin != null);
      Debug.Assert(baseCoin != quoteCoin);
      Debug.Assert(baseCoin != Coin.FromName("Ark"));
      Debug.Assert(askPriceOrOfferYouCanBuy >= 0);
      Debug.Assert(bidPriceOrOfferYouCanSell >= 0);
      Debug.Assert(exchange.supportsOverlappingBooks
        || askPriceOrOfferYouCanBuy == 0
        || bidPriceOrOfferYouCanSell == 0
        || askPriceOrOfferYouCanBuy >= bidPriceOrOfferYouCanSell);

      this.exchange = exchange;
      this.baseCoin = baseCoin;
      this.quoteCoin = quoteCoin;
      this.askPriceOrOfferYouCanBuy = askPriceOrOfferYouCanBuy;
      this.bidPriceOrOfferYouCanSell = bidPriceOrOfferYouCanSell;
      this.lastUpdated = DateTime.Now;
      this.isInactive = isInactive;
      this.orderBook = new OrderBook(this);
    }
    #endregion

    #region Public API
    /// <summary>Get's the most recent successful trade for this pair.</summary>
    /// <param name="maxCacheAgeInSeconds">
    /// Use the cache unless the last refresh was more than this ago.
    /// </param>
    // TODO move to last trade?
    public async Task<LastTrade> GetLastTradeAsync(
      int maxCacheAgeInSeconds)
    {
      if ((DateTime.Now - lastTrade.dateCreated).TotalSeconds > maxCacheAgeInSeconds)
      {
        await exchange.RefreshLastTrade(this);
      }

      return lastTrade;
    }
    #endregion

    #region Internal Write API
    internal void Update(
      decimal askPriceOrOfferYouCanBuy,
      decimal bidPriceOrOfferYouCanSell)
    {
      Debug.Assert(askPriceOrOfferYouCanBuy >= 0);
      Debug.Assert(bidPriceOrOfferYouCanSell >= 0);
      Debug.Assert(exchange.supportsOverlappingBooks
        || askPriceOrOfferYouCanBuy == 0
        || bidPriceOrOfferYouCanSell == 0
        || askPriceOrOfferYouCanBuy >= bidPriceOrOfferYouCanSell);

      this.askPriceOrOfferYouCanBuy = askPriceOrOfferYouCanBuy;
      this.bidPriceOrOfferYouCanSell = bidPriceOrOfferYouCanSell;
      this.lastUpdated = DateTime.Now;
      onPriceUpdate?.Invoke();
    }
    #endregion

    #region Class Stuff
    public override string ToString()
    {
      return $"{quoteCoin}-{baseCoin} {bidPriceOrOfferYouCanSell}-{askPriceOrOfferYouCanBuy}";
    }
    #endregion
  }
}