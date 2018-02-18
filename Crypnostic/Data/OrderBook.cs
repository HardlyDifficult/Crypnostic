using System;
using System.Diagnostics;

namespace Crypnostic
{
  public struct OrderBook
  {
    public readonly Order[] asks;

    public readonly Order[] bids;

    public readonly DateTime dateCreated;

    public OrderBook(
      Order[] asks,
      Order[] bids)
    {
      DebugAssertSortedPrices(asks, true);
      DebugAssertSortedPrices(bids, false);

      this.asks = asks;
      this.bids = bids;
      this.dateCreated = DateTime.Now;
    }

    [Conditional("DEBUG")]
    static void DebugAssertSortedPrices(
      Order[] orders,
      bool isGoingUp)
    {
      if(orders.Length == 0)
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
