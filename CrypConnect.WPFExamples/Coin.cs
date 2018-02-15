using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ExampleWPF
{
  public class Coin : INotifyPropertyChanged
  {
    private string _fullName;
    private string _name;

    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        OnPropertyChanged(); //this will make it update everywhere this property is binded
      }
    }

    public string FullName
    {
      get { return _fullName; }
      set
      {
        _fullName = value;
        OnPropertyChanged();//this will make it update everywhere this property is binded

      }

    }

    public decimal price
    {
      get
      {
        return new Random().Next();
      }
    }

    public void OnPriceChange()
    {
      OnPropertyChanged(nameof(price));
    }

    public Coin()
    {
      Timer timer = new Timer();
      timer.Interval = 1000;
      timer.Elapsed += Timer_Elapsed;
      timer.AutoReset = true;
      timer.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      OnPriceChange();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
