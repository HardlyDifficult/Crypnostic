using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CrypConnect
{
  public class TradingPair
  {
    #region Data
    // TODO config?  Should this match the price refresh time?
    TimeSpan timeBetweenLastTradeUpdates = TimeSpan.FromSeconds(15);

    public event Action onPriceUpdate;

    public event Action onStatusChange;

    public readonly Exchange exchange;

    public readonly Coin baseCoin;

    public readonly Coin quoteCoin;

    internal LastTrade lastTrade
    {
      private get; set;
    }

    public async Task<LastTrade> GetLastTrade()
    {
      if (DateTime.Now - lastTrade.dateCreated > timeBetweenLastTradeUpdates)
      {
        await exchange.RefreshLastTrade(this);
      }

      return lastTrade;
    }

    /// <summary>
    /// The cost to purchase.
    /// </summary>
    public decimal askPrice
    {
      get; private set;
    }

    /// <summary>
    /// The price you would get by selling.
    /// </summary>
    public decimal bidPrice
    {
      get; private set;
    }

    public DateTime lastUpdated
    {
      get; private set;
    }
    #endregion


    bool _isInactive;

    public bool isInactive
    {
      get
      {
        return _isInactive || askPrice == 0 && bidPrice == 0;
      }
      set
      {
        if (isInactive == value)
        { // No change
          return;
        }

        _isInactive = value;
        onStatusChange?.Invoke();
      }
    }

    #region Init
    internal TradingPair(
      Exchange exchange,
      Coin baseCoin,
      Coin quoteCoin,
      decimal askPrice,
      decimal bidPrice,
      bool isInactive = false)
    {
      Debug.Assert(baseCoin != Coin.FromName("Ark"));
      Debug.Assert(askPrice >= 0);
      Debug.Assert(bidPrice >= 0);

      this.exchange = exchange;
      this.baseCoin = baseCoin;
      this.quoteCoin = quoteCoin;
      this.askPrice = askPrice;
      this.bidPrice = bidPrice;
      this.lastUpdated = DateTime.Now;
      this.isInactive = isInactive;

      quoteCoin.AddPair(this);
    }
    #endregion

    #region Internal Write API
    internal void Update(
      decimal askPrice,
      decimal bidPrice)
    {
      this.askPrice = askPrice;
      this.bidPrice = bidPrice;
      this.lastUpdated = DateTime.Now;
      onPriceUpdate?.Invoke();
    }
    #endregion

    #region Class Stuff
    public override string ToString()
    {
      return $"{quoteCoin}/{baseCoin} {bidPrice}-{askPrice}";
    }
    #endregion
  }
}