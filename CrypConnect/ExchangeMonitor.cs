using CryptoExchanges.CoinMarketCap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CryptoExchanges
{
  public class ExchangeMonitor
  {
    #region Internal/Private Data
    internal static ExchangeMonitor instance;

    internal readonly Random random = new Random();

    /// <summary>
    /// In priority order, so first exchange is my most preferred trading platform.
    /// </summary>
    readonly Exchange[] exchangeList;

    internal bool shouldStop
    {
      get; private set;
    }
    #endregion

    #region Init
    public ExchangeMonitor(
      ExchangeMonitorConfig config)
    {
      Debug.Assert(instance == null);
      instance = this;

      Debug.Assert(Coin.aliasLowerToCoin != null);
      foreach (KeyValuePair<string, string> aliasToName in config.coinAliasToName)
      {
        AddAlias(aliasToName.Key, aliasToName.Value);
      }

      exchangeList = new Exchange[config.supportedExchangeList.Length];
      for (int i = 0; i < config.supportedExchangeList.Length; i++)
      {
        ExchangeName name = config.supportedExchangeList[i];
        Exchange exchange = Exchange.LoadExchange(this, name, config.includeMaintainceStatus);
        exchangeList[i] = exchange;
      }

      foreach (string blacklistedCoin in config.blacklistedCoins)
      {
        Coin.blacklistedFullNameLowerList.Add(blacklistedCoin.ToLowerInvariant());
      }

      CompleteFirstLoad().Wait();
    }

   

    async Task CompleteFirstLoad()
    {
      List<Task> taskList = new List<Task>();

      for (int i = 0; i < exchangeList.Length; i++)
      {
        taskList.Add(exchangeList[i].GetAllPairs());
      }

      await Task.WhenAll(taskList);
    }

    public void Stop()
    {
      Debug.Assert(instance == this);

      shouldStop = true;
      instance = null;
    }
    #endregion

    public static void AddAlias(
     string alias,
     string name)
    {
      alias = alias.ToLowerInvariant();
      Debug.Assert(Coin.fullNameLowerToCoin.ContainsKey(alias) == false);

      if(Coin.aliasLowerToCoin.ContainsKey(alias))
      { // De-dupe
        return;
      }

      name = name.ToLowerInvariant();
      Coin coin = Coin.FromName(name);

      Coin.aliasLowerToCoin.Add(alias, coin);
    }

    #region Public Read API
    public Exchange FindExchange(
      ExchangeName onExchange)
    {
      for (int i = 0; i < exchangeList.Length; i++)
      {
        Exchange exchange = exchangeList[i];
        if (exchange.exchangeName == onExchange)
        {
          return exchange;
        }
      }

      return null;
    }
    #endregion
  }
}
