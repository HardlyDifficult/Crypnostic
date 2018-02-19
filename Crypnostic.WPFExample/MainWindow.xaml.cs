using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Crypnostic.WPFExamples
{
  public partial class MainWindow : Window
  {
    CrypnosticController exchangeMonitor;

    readonly CrypDataContext dataContext = new CrypDataContext();

    public MainWindow()
    {
      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += Worker_DoWork;
      worker.RunWorkerAsync();

      InitializeComponent();
      DataContext = dataContext; 
    }

    async void Worker_DoWork(
      object sender, 
      DoWorkEventArgs e)
    {
      CrypnosticConfig config = new CrypnosticConfig(
        ExchangeName.Binance,
        ExchangeName.Cryptopia,
        ExchangeName.Kucoin);
      exchangeMonitor = new CrypnosticController(config);
      await exchangeMonitor.Start();
    }

    void AddButton_OnClick(
      object sender, 
      RoutedEventArgs e)
    {
      dataContext.Add(CoinName.Text);
    }
  }
}
