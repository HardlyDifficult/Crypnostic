using System;

namespace CryptoExchanges
{
  public class TradingPair
  {
    #region Data
    public event Action onPriceUpdate;

    public event Action onStatusChange;

    public readonly Exchange exchange;

    public readonly Coin baseCoin;

    public readonly Coin quoteCoin;


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
        return _isInactive;
      }
      set
      {
        if(isInactive == value)
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
      decimal AskPrice,
      decimal BidPrice)
    {
      this.exchange = exchange;
      this.baseCoin = baseCoin;
      this.quoteCoin = quoteCoin;
      this.askPrice = AskPrice;
      this.bidPrice = BidPrice;
      this.lastUpdated = DateTime.Now;

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