using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Crypnostic
{
  public class OrderBook
  {
    public readonly TradingPair tradingPair;

    public Order[] asksOrOffersYouCanBuy
    {
      get; private set;
    }

    public Order[] bidsOrOffersYouCanSell
    {
      get; private set;
    }

    public DateTime lastUpdated
    {
      get; private set;
    }

    Action<OrderBook> _onUpdate;
    
    /// <summary>
    /// By subscribing to this event you are enabling auto-updates
    /// on this book, which occur each time the exchange updates prices.
    /// 
    /// Note that updating the OrderBook requires an API call for each book
    /// individually, so limit subs to maintain good performance.
    /// 
    /// Unsubscribe to stop the auto-updates for this book.
    /// </summary>
    public event Action<OrderBook> onUpdate
    {
      add
      {
        if(_onUpdate == null)
        {
          tradingPair.exchange.autoUpdatingBooks.Add(this);
        }
        _onUpdate += value;
      }
      remove
      {
        _onUpdate -= value;

        if (_onUpdate == null)
        {
          tradingPair.exchange.autoUpdatingBooks.Remove(this);
        }
      }
    }

    internal OrderBook(
      TradingPair tradingPair)
    {
      Debug.Assert(tradingPair != null);

      this.tradingPair = tradingPair;
    }

    /// <summary>
    /// Gets approx 100 bids and asks, allowing you to review market depth for a trading pair.
    /// </summary>
    public async Task RefreshAsync()
    {
      await tradingPair.exchange.UpdateOrderBook(this);
    }

    internal void Update(
      Order[] asksOrOffersYouCanBuy,
      Order[] bidsOrOffersYouCanSell)
    {
      DebugAssertSortedPrices(asksOrOffersYouCanBuy, true);
      DebugAssertSortedPrices(bidsOrOffersYouCanSell, false);

      this.asksOrOffersYouCanBuy = asksOrOffersYouCanBuy;
      this.bidsOrOffersYouCanSell = bidsOrOffersYouCanSell;
      this.lastUpdated = DateTime.Now;

      _onUpdate?.Invoke(this);
    }

    [Conditional("DEBUG")]
    static void DebugAssertSortedPrices(
      Order[] orders,
      bool isGoingUp)
    {
      if (orders.Length == 0)
      {
        return;
      }

      for (int i = 1; i < orders.Length; i++)
      {
        Debug.Assert(isGoingUp && orders[1].price > orders[0].price
          || isGoingUp == false && orders[1].price < orders[0].price);
      }
    }
  }
}
