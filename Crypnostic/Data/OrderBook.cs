using System;
using System.Diagnostics;

namespace Crypnostic
{
  public struct OrderBook
  {
    public readonly Order[] asksOrOffersYouCanBuy;

    public readonly Order[] bidsOrOffersYouCanSell;

    public readonly DateTime dateCreated;

    public OrderBook(
      Order[] asksOrOffersYouCanBuy,
      Order[] bidsOrOffersYouCanSell)
    {
      DebugAssertSortedPrices(asksOrOffersYouCanBuy, true);
      DebugAssertSortedPrices(bidsOrOffersYouCanSell, false);

      this.asksOrOffersYouCanBuy = asksOrOffersYouCanBuy;
      this.bidsOrOffersYouCanSell = bidsOrOffersYouCanSell;
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
