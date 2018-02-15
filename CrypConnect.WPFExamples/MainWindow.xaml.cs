using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExampleWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WatchedCoin wc = new WatchedCoin();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = wc; // Setting datacontext to the watchcoin thing, cant remember what class u wanted it as.
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            wc.Add(CoinName.Text,CoinFullName.Text);
        }

        private void SwapNameToFullName_OnClick(object sender, RoutedEventArgs e)
        {
            wc.SwapNameToFullNameOnFirstItem();
        }
    }
}
