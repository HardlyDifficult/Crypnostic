using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crypnostic.Tools
{
  public static class CoinTools
  {
    /// <summary>
    /// </summary>
    /// <param name="sellVsBuy">
    /// True: Sell this coin for baseCoin. False: Buy this coin with baseCoin.
    /// </param>
    /// <param name="baseCoinFullName"></param>
    /// <param name="exchangeName">
    /// If specified, only consider trades on this exchange
    /// </param>
    /// <returns></returns>
    public static TradingPair Best(
      Coin quoteCoin,
      Coin baseCoin,
      bool sellVsBuy,
      ExchangeName? exchangeName = null)
    {
      TradingPair bestPair = null;
      decimal? bestValue = null;
      foreach (TradingPair pair in quoteCoin.allTradingPairs)
      {
        if (exchangeName != null && pair.exchange.exchangeName != exchangeName.Value)
        { // Filter by exchange (optional)
          continue;
        }

        if (pair.baseCoin != baseCoin)
        { // Filter by baseCoin (optional)
          continue;
        }

        if (pair.isInactive)
        { // Ignore inactive pairs
          continue;
        }

        decimal value = sellVsBuy ? pair.bidPriceOrOfferYouCanSell : pair.askPriceOrOfferYouCanBuy;
        if (value <= 0)
        { // No bid/ask to consider here
          continue;
        }

        if (bestValue == null
          || sellVsBuy && value > bestValue.Value
          || sellVsBuy == false && value < bestValue.Value)
        {
          bestValue = value;
          bestPair = pair;
        }
      }

      return bestPair;
    }

    // TODO no tuple?
    public static async Task<(decimal purchaseAmount, decimal quantity)> CalcPurchasePrice(
      Coin quoteCoin,
      ExchangeName askExchange,
      Coin askBaseCoin,
      decimal purchasePriceInBase)
    {
      decimal purchaseAmount = 0;
      decimal quantity = 0;

      Exchange exchange = CrypnosticController.instance.GetExchange(askExchange);
      OrderBook orderBook = await exchange.GetOrderBook(quoteCoin, askBaseCoin);

      for (int i = 0; i < orderBook.asksOrOffersYouCanBuy.Length; i++)
      {
        Order order = orderBook.asksOrOffersYouCanBuy[i];
        decimal purchaseAmountFromOrder = Math.Min(purchasePriceInBase,
          order.price * order.volume);

        purchaseAmount += purchaseAmountFromOrder;
        quantity += purchaseAmountFromOrder / order.price;
        purchasePriceInBase -= purchaseAmountFromOrder;

        if (purchasePriceInBase <= 0)
        {
          break;
        }
      }

      return (purchaseAmount, quantity);
    }

    public static async Task<decimal> CalcSellPrice(
      Coin quoteCoin,
      ExchangeName bidExchange,
      Coin bidBaseCoin,
      decimal quantityOfCoin)
    {
      decimal sellAmount = 0;

      Exchange exchange = CrypnosticController.instance.GetExchange(bidExchange);
      OrderBook orderBook = await exchange.GetOrderBook(quoteCoin, bidBaseCoin);

      for (int i = 0; i < orderBook.bidsOrOffersYouCanSell.Length; i++)
      {
        Order order = orderBook.bidsOrOffersYouCanSell[i];
        decimal sellAmountFromOrder = Math.Min(quantityOfCoin, order.volume);

        sellAmount += sellAmountFromOrder * order.price;
        quantityOfCoin -= sellAmountFromOrder;

        if (quantityOfCoin <= 0)
        {
          break;
        }
      }

      return sellAmount;
    }

  }
}
