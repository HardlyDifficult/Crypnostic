using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrypConnect.WPFExamples
{
  public abstract class WpfDataContext : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(
      [CallerMemberName]
      string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
