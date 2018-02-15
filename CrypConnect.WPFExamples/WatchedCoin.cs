using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExampleWPF
{
    public class WatchedCoin:INotifyPropertyChanged
    {
        private List<Coin> _coins; //reason i use a list to back the observable collection is cause i have had trouble adding stuff to the observable collection so instead i use it like this.

        public WatchedCoin()
        {
            _coins = new List<Coin>();
        }

        public ObservableCollection<Coin> Coins
        {
            get { return new ObservableCollection<Coin>(_coins);}
            set { _coins = value.ToList(); }
        }

        public void Add(string name, string fullName)
        {
            _coins.Add(new Coin(){ Name = name, FullName = fullName});

            OnPropertyChanged("Coins"); // using this will update the observable collection 
        }



        //Just swaps the name and  full name to display how to update an item by just seting the property.
        public void SwapNameToFullNameOnFirstItem()
        {
           var firstCoin = _coins.FirstOrDefault();
            if (firstCoin == null)
            {
                return;
            }

            var name = firstCoin.Name;
            firstCoin.Name = firstCoin.FullName;
            firstCoin.FullName = name;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
