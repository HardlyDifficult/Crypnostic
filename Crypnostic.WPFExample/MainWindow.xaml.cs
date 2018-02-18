using System;
using System.ComponentModel;
using System.Windows;

namespace Crypnostic.WPFExamples
{
  public partial class MainWindow : Window
  {
    ExchangeMonitor exchangeMonitor;

    readonly CrypDataContext dataContext = new CrypDataContext();

    public MainWindow()
    {
      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += Worker_DoWork;
      worker.RunWorkerAsync();

      InitializeComponent();
      DataContext = dataContext; 
    }

    void Worker_DoWork(
      object sender, 
      DoWorkEventArgs e)
    {
      ExchangeMonitorConfig config = new ExchangeMonitorConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin);
      exchangeMonitor = new ExchangeMonitor(config);
    }

    void AddButton_OnClick(
      object sender, 
      RoutedEventArgs e)
    {
      dataContext.Add(CoinName.Text);
    }
  }
}
