using Common.Logging;
using Crypnostic.CoinMarketCap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Crypnostic
{
  /// <summary>
  /// This is the main class for interfacing with Crypnostic.
  /// </summary>
  public class CrypnosticController
  {
    #region Public Data
    /// <summary>
    /// Called anytime a brand new Coin has been detected.
    /// 
    /// If you subscribe before calling Start(), the event 
    /// will report every Coin.
    /// </summary>
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
    public static CrypnosticController instance;

    /// <summary>
    /// Canceled when Stop() is called.
    /// </summary>
    internal readonly CancellationTokenSource cancellationTokenSource
      = new CancellationTokenSource();

    /// <summary>
    /// Populated from config on construction.
    /// 
    /// Note the Coin for an alias may be null (e.g. if it's blacklisted)
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

    static readonly ILog log = LogManager.GetLogger<CrypnosticController>();

    /// <summary>
    /// In priority order, so first exchange is my most preferred trading platform.
    /// </summary>
    readonly Exchange[] exchangeList;

    internal readonly CoinMarketCapAPI coinMarketCap;
    #endregion

    #region Init
    /// <summary>
    /// Before using this controller, call Start().
    /// </summary>
    public CrypnosticController(
      CrypnosticConfig config)
    {
      log.Trace(nameof(CrypnosticController));

      Debug.Assert(config != null);
      Debug.Assert(config.supportedExchangeList.Length > 0);
      Debug.Assert(instance == null);
      instance = this;

      coinMarketCap = new CoinMarketCapAPI(); 

      foreach (KeyValuePair<string, string> aliasToName in config.coinAliasToName)
      {
        AddCoinAlias(aliasToName.Value, aliasToName.Key);
      }

      foreach (string blacklistedCoin in config.blacklistedCoins)
      {
        Debug.Assert(fullNameLowerToCoin.ContainsKey(blacklistedCoin.ToLowerInvariant()) == false);
        blacklistedFullNameLowerList.Add(blacklistedCoin.ToLowerInvariant());
      }

      exchangeList = new Exchange[config.supportedExchangeList.Length];
      for (int i = 0; i < config.supportedExchangeList.Length; i++)
      {
        ExchangeName name = config.supportedExchangeList[i];
        Exchange exchange = Exchange.LoadExchange(name);
        exchangeList[i] = exchange;
      }
    }

    /// <summary>
    /// Completes an initial download from every exchange (before returning)
    /// and then starts auto-refreshing.
    /// </summary>
    public async Task Start()
    {
      log.Trace(nameof(Start));

      List<Task> taskList = new List<Task>();

      taskList.Add(coinMarketCap.Start());
      for (int i = 0; i < exchangeList.Length; i++)
      {
        taskList.Add(exchangeList[i].Start());
      }

      await Task.WhenAll(taskList);
    }

    /// <summary>
    /// Call this when exiting the application
    /// or any other time you want the auto-refresh to stop.
    /// </summary>
    public void Stop()
    {
      log.Trace(nameof(Stop));

      Debug.Assert(instance == this);

      cancellationTokenSource.Cancel();
      instance = null;
    }
    #endregion

    #region Events
    internal void OnNewCoin(
      Coin coin)
    {
      Debug.Assert(coin != null);
      // TODO
      //Debug.Assert(fullNameLowerToCoin.ContainsKey(coin.fullNameLower) == false);

      fullNameLowerToCoin[coin.fullNameLower] = coin;
      onNewCoin?.Invoke(coin);
    }
    #endregion

    #region Public Write API
    /// <summary>
    /// Aliases will take a coin's full name and map it to another.
    /// When in doubt, use the full name from CoinMarketCap.com
    /// 
    /// Currently various Exchange implementations add some aliases they require.
    /// 
    /// Be careful: too aggressive leads incorrect matches, not enough 
    /// and you may miss opportunities.
    /// </summary>
    public void AddCoinAlias(
      string fullName,
      string alias)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(alias) == false);
      Debug.Assert(string.IsNullOrWhiteSpace(fullName) == false);
      Debug.Assert(alias.Equals(fullName, StringComparison.InvariantCultureIgnoreCase) == false);
      Debug.Assert(fullNameLowerToCoin.ContainsKey(alias.ToLowerInvariant()) == false);

      alias = alias.ToLowerInvariant();
      if (aliasLowerToCoin.ContainsKey(alias))
      { // De-dupe
        return;
      }

      Coin coin = Coin.CreateFromName(fullName);
        aliasLowerToCoin.Add(alias, coin);
    }

    /// <summary>
    /// Defines aliases for a coin.  
    /// e.g. TetherUS maps to Tether so it matches with other exchanges.
    /// </summary>
    /// <param name="coinFullNameToAliasList">
    /// The first string is the coin's official full name we will use going forward.
    /// The other strings are aliases which will map to the first.
    /// 
    /// When in doubt, use the full name from CoinMarketCap.
    /// </param>
    public void AddCoinAlias(
      params string[][] coinFullNameToAliasList)
    {
      Debug.Assert(coinFullNameToAliasList != null);
      Debug.Assert(coinFullNameToAliasList.Length > 0);

      for (int i = 0; i < coinFullNameToAliasList.Length; i++)
      {
        string[] coinFullNameToAlias = coinFullNameToAliasList[i];
        Debug.Assert(coinFullNameToAlias != null);
        Debug.Assert(coinFullNameToAlias.Length > 1);

        string primaryFullName = coinFullNameToAlias[0];
        Debug.Assert(string.IsNullOrWhiteSpace(primaryFullName) == false);

        for (int j = 1; j < coinFullNameToAlias.Length; j++)
        {
          string alias = coinFullNameToAlias[j];
          Debug.Assert(string.IsNullOrWhiteSpace(alias) == false);
          Debug.Assert(alias.Equals(primaryFullName,
            StringComparison.InvariantCultureIgnoreCase) == false);

          AddCoinAlias(primaryFullName, alias);
        }
      }
    }
    #endregion

    #region Public Read API
    public Exchange GetExchange(
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
