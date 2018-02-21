using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Crypnostic
{
  /// <summary>
  /// The last price and the time it was last refreshed.
  /// 
  /// Different exchanges will update this value at different rates.
  /// </summary>
  public class LastTrade
  {
    public readonly TradingPair tradingPair;

    /// <summary>
    /// Price of 0 means no recent trade
    /// (different than an unknown state)
    /// </summary>
    public decimal price
    {
      get; private set;
    }

    public DateTime lastUpdated
    {
      get; private set;
    }

    bool _autoRefresh;

    Action<LastTrade> _onUpdate;

    /// <summary>
    /// Note that updating the LastTrade requires an API call for each book 
    /// on some exchanges (others get this info for 'free' with other calls)
    /// 
    /// Limit subs to maintain good performance.
    /// </summary>
    public bool autoRefresh
    {
      get
      {
        return _autoRefresh || _onUpdate != null;
      }
      set
      {
        if (_autoRefresh == value)
        {
          return;
        }

        _autoRefresh = value;

        if (autoRefresh)
        {
          tradingPair.exchange.autoLastTrades.Add(this);
        }
        else
        {
          tradingPair.exchange.autoLastTrades.Remove(this);
        }
      }
    }

    /// <summary>
    /// By subscribing to this event you are enabling auto-updates
    /// on this book, which occur each time the exchange updates prices.
    /// 
    /// Note that updating the LastTrade requires an API call for each book 
    /// on some exchanges (others get this info for 'free' with other calls)
    /// 
    /// Limit subs to maintain good performance.
    /// 
    /// Unsubscribe to stop the auto-updates for this book.
    /// </summary>
    public event Action<LastTrade> onUpdate
    {
      add
      {
        if (_onUpdate == null && _autoRefresh == false)
        {
          tradingPair.exchange.autoLastTrades.Add(this);
        }

        _onUpdate += value;
      }
      remove
      {
        _onUpdate -= value;

        if (_onUpdate == null && _autoRefresh == false)
        {
          tradingPair.exchange.autoLastTrades.Remove(this);
        }
      }
    }

    internal LastTrade(
      TradingPair tradingPair)
    {
      this.tradingPair = tradingPair;
    }

    internal void Update(
      decimal price)
    {
      this.price = price;
      this.lastUpdated = DateTime.Now;

      _onUpdate?.Invoke(this);
    }

    #region Public API
    /// <summary>Get's the most recent successful trade for this pair.</summary>
    /// <param name="maxCacheAgeInSeconds">
    /// Use the cache unless the last refresh was more than this ago.
    /// </param>
    public async Task RefreshAsync()
    {
      await tradingPair.exchange.RefreshLastTrade(tradingPair);
    }
    #endregion
  }
}
