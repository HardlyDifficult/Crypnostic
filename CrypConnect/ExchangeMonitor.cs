using CrypConnect.CoinMarketCap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CrypConnect
{
  public class ExchangeMonitor
  {
    #region Public Data
    public event Action<Coin> onNewCoin;

    public IEnumerable<Coin> allCoins
    {
      get
      {
        return fullNameLowerToCoin.Values;
      }
    }
    #endregion

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

    /// <summary>
    /// Populated from config on construction.
    /// </summary>
    internal readonly Dictionary<string, Coin> aliasLowerToCoin
      = new Dictionary<string, Coin>();

    /// <summary>
    /// Populated from config on construction.
    /// After consider aliases.
    /// </summary>
    internal readonly HashSet<string> blacklistedFullNameLowerList
      = new HashSet<string>();

    /// <summary>
    /// After considering aliases and blacklist.
    /// </summary>
    internal readonly Dictionary<string, Coin> fullNameLowerToCoin
      = new Dictionary<string, Coin>();
    #endregion

    #region Init
    public ExchangeMonitor(
      ExchangeMonitorConfig config)
    {
      Debug.Assert(instance == null);
      instance = this;

      foreach (KeyValuePair<string, string> aliasToName in config.coinAliasToName)
      {
        AddAlias(aliasToName.Key, aliasToName.Value);
      }

      exchangeList = new Exchange[config.supportedExchangeList.Length];
      for (int i = 0; i < config.supportedExchangeList.Length; i++)
      {
        ExchangeName name = config.supportedExchangeList[i];
        Exchange exchange = Exchange.LoadExchange(this, name);
        exchangeList[i] = exchange;
      }

      foreach (string blacklistedCoin in config.blacklistedCoins)
      {
        blacklistedFullNameLowerList.Add(blacklistedCoin.ToLowerInvariant());
      }

      CompleteFirstLoad().Wait();
    }

    async Task CompleteFirstLoad()
    {
      List<Task> taskList = new List<Task>();

      for (int i = 0; i < exchangeList.Length; i++)
      {
        taskList.Add(exchangeList[i].GetAllPairs(true));
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

    #region Events
    internal void OnNewCoin(
      Coin coin)
    {
      fullNameLowerToCoin.Add(coin.fullNameLower, coin);
      onNewCoin?.Invoke(coin);
    }
    #endregion

    #region Public Write API
    public void AddAlias(
     string alias,
     string name)
    {
      alias = alias.ToLowerInvariant();
      Debug.Assert(fullNameLowerToCoin.ContainsKey(alias) == false);

      if(aliasLowerToCoin.ContainsKey(alias))
      { // De-dupe
        return;
      }

      name = name.ToLowerInvariant();
      Coin coin = Coin.CreateFromName(name);

      aliasLowerToCoin.Add(alias, coin);
    }
    #endregion

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
