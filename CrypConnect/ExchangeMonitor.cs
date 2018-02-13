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

      exchangeList = new Exchange[config.supportedExchangeList.Length];
      for (int i = 0; i < config.supportedExchangeList.Length; i++)
      {
        ExchangeName name = config.supportedExchangeList[i];
        Exchange exchange = Exchange.LoadExchange(this, name);
        exchangeList[i] = exchange;
      }

      foreach (KeyValuePair<string, string> aliasToName in config.coinAliasToName)
      {
        Debug.Assert(Coin.aliasLowerToFullNameLower != null);
        Coin.aliasLowerToFullNameLower.Add(aliasToName.Key.ToLowerInvariant(),
          aliasToName.Value.ToLowerInvariant());
      }

      foreach (string blacklistedCoin in config.blacklistedCoins)
      {
        Coin.blacklistedFullNameLowerList.Add(blacklistedCoin.ToLowerInvariant());
      }

      CompleteFirstLoad().Wait();
    }

    async Task CompleteFirstLoad()
    {
      Task[] exchangeTaskList = new Task[exchangeList.Length];
      for (int i = 0; i < exchangeList.Length; i++)
      {
        exchangeTaskList[i] = exchangeList[i].GetAllPairs();
      }

      await Task.WhenAll(exchangeTaskList);
    }

    public void Stop()
    {
      Debug.Assert(instance == this);

      // TODO cancel token on pending requests where we can?
      shouldStop = true;
      instance = null;
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
