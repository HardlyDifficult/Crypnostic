﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Crypnostic.Tools
{
  public static class TradingPairExtensions
  {
    // TODO no tuple?
    public static async Task<(decimal purchaseAmount, decimal quantity)> CalcPurchasePrice(
      this TradingPair tradingPair,
      decimal purchasePriceInBase)
    {
      decimal purchaseAmount = 0;
      decimal quantity = 0;

      OrderBook orderBook = tradingPair.orderBook;

      try
      {
        await orderBook.RefreshAsync();
      }
      catch
      {
        // Temp assert
        Debug.Assert(tradingPair.exchange.exchangeName == ExchangeName.EtherDelta);

        return (purchasePriceInBase, purchasePriceInBase / tradingPair.askPriceOrOfferYouCanBuy);
      }

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
      this TradingPair tradingPair,
      decimal quantityOfCoin)
    {
      decimal sellAmount = 0;

      OrderBook orderBook = tradingPair.orderBook;
      try
      {
      await orderBook.RefreshAsync();
      }
      catch
      {
        // Temp assert
        Debug.Assert(tradingPair.exchange.exchangeName == ExchangeName.EtherDelta);

        return quantityOfCoin * tradingPair.bidPriceOrOfferYouCanSell;
      }

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
