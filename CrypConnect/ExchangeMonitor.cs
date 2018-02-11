using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  public class ExchangeMonitor
  {
    public static readonly Random random = new Random();

    /// <summary>
    /// In priority order, so first exchange is my most preferred trading platform.
    /// </summary>
    readonly Exchange[] exchangeList;

    /// <summary>
    /// CoinFullName (lowercase) to Coin
    /// </summary>
    readonly Dictionary<string, Coin> coinList = new Dictionary<string, Coin>();

    readonly HashSet<string> blacklistedCoinFullNameList = new HashSet<string>();

    public ExchangeMonitor(
      params ExchangeName[] exchangeNameList)
    {
      exchangeList = new Exchange[exchangeNameList.Length];
      for (int i = 0; i < exchangeNameList.Length; i++)
      {
        ExchangeName name = exchangeNameList[i];
        Exchange exchange = Exchange.LoadExchange(this, name);
        exchangeList[i] = exchange;
      }
    }

    public void BlacklistCoins(
      params string[] coinFullNames)
    {
      for (int i = 0; i < coinFullNames.Length; i++)
      {
        string coinName = coinFullNames[i];
        if (blacklistedCoinFullNameList.Add(coinName))
        {
          coinList.Remove(coinName);
        }
      }

    }

    public Coin FindCoin(
      string coinFullName)
    {
      if (coinList.TryGetValue(coinFullName.ToLowerInvariant(), out Coin coin) == false)
      {
        return null;
      }

      return coin;
    }

    public async Task CompleteFirstLoad()
    {
      Task[] exchangeTaskList = new Task[exchangeList.Length];
      for (int i = 0; i < exchangeList.Length; i++)
      {
        exchangeTaskList[i] = exchangeList[i].GetAllPairs();
      }

      await Task.WhenAll(exchangeTaskList);
    }

    public void AddPair(
      TradingPair pair)
    {
      if (blacklistedCoinFullNameList.Contains(pair.quoteCoinFullName)
        || blacklistedCoinFullNameList.Contains(pair.baseCoinFullName))
      {
        return;
      }

      if (coinList.TryGetValue(pair.quoteCoinFullName.ToLowerInvariant(), out Coin coin) == false)
      {
        coin = new Coin(pair.quoteCoinFullName);
        coinList.Add(pair.quoteCoinFullName.ToLowerInvariant(), coin);
      }
      coin.AddPair(pair);
    }
  }
}
