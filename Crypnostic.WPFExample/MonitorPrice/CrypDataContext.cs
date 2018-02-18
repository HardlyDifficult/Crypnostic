using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Crypnostic.WPFExamples
{
  public class CrypDataContext : WpfDataContext
  {
    public BindingList<WatchedCoin> coinList
    {
      get;
    } = new BindingList<WatchedCoin>();

    public void Add(
      string name)
    {
      Coin coin = Coin.FromName(name);
      if(coin == null)
      {
        MessageBox.Show($"Can't find {name} coin");
        return;
      }

      coinList.Add(new WatchedCoin(coin));
      OnPropertyChanged(nameof(coinList)); 
    }
  }
}
