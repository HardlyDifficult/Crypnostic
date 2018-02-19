using Crypnostic.Tools;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Crypnostic.WPFExamples
{
  public class WatchedCoin : WpfDataContext
  {
    #region Data
    readonly Coin coin;
    #endregion

    #region Properties
    public string shortText
    {
      get
      {
        TradingPair pair = coin.FindBestOffer(Coin.bitcoin, OrderType.Sell);
        return $"{coin.fullName} {pair?.bidPriceOrOfferYouCanSell} BTC on {pair?.exchange.exchangeName}";
      }
    }
    #endregion

    #region Init
    public WatchedCoin(
      Coin coin)
    {
      this.coin = coin;
      coin.onPriceUpdate += Coin_onPriceUpdate;
    }
    #endregion

    #region Events
    void Coin_onPriceUpdate(
      Coin coin, 
      TradingPair tradingPair)
    {
      OnPropertyChanged(nameof(shortText));
    }
    #endregion
  }
}
