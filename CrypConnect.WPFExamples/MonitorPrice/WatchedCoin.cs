using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace CrypConnect.WPFExamples
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
        TradingPair pair = coin.Best(Coin.bitcoin, true);
        return $"{coin.fullName} {pair?.bidPrice} BTC on {pair?.exchange.exchangeName}";
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
