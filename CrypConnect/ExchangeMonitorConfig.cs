using System;
using System.Collections.Generic;
using System.Diagnostics;
using CryptoExchanges;

namespace CryptoExchanges
{
  [Serializable]
  public class ExchangeMonitorConfig
  {
    public  bool includeMaintainceStatus;

    internal readonly ExchangeName[] supportedExchangeList;
    
    internal readonly Dictionary<string, string> coinAliasToName 
      = new Dictionary<string, string>();

    internal readonly HashSet<string> blacklistedCoins 
      = new HashSet<string>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="supportedExchangeList">
    /// List in priority order.  i.e. the first exchange will be considered before others.
    /// Leave blank to support every exchange.
    /// </param>
    public ExchangeMonitorConfig(
      params ExchangeName[] supportedExchangeList)
    {
      if(supportedExchangeList.Length == 0)
      {
        supportedExchangeList = (ExchangeName[])Enum.GetValues(typeof(ExchangeName));
      }

      this.supportedExchangeList = supportedExchangeList;
    }

    /// <summary>
    /// Defines aliases for a coin.  
    /// e.g. TetherUS maps to Tether so it matches with other exchanges.
    /// </summary>
    /// <param name="coinFullNameMapList">
    /// The first string is the coin's full name we will use going forward.
    /// The other strings are aliases which will map to the first.
    /// </param>
    public void AddCoinMap(
      params string[][] coinFullNameMapList)
    {
      for (int i = 0; i < coinFullNameMapList.Length; i++)
      {
        string[] coinFullNameMap = coinFullNameMapList[i];
        Debug.Assert(coinFullNameMap.Length > 1);

        string primaryFullName = coinFullNameMap[0];
        for (int j = 1; j < coinFullNameMap.Length; j++)
        {
          string alias = coinFullNameMap[j];
          coinAliasToName.Add(alias, primaryFullName);
        }
      }
    }

    /// <summary>
    /// Completely removes these coins from consideration.
    /// To restore these entries, stop and restart the program.
    /// </summary>
    public void BlacklistCoins(
      params string[] coinFullNames)
    {
      for (int i = 0; i < coinFullNames.Length; i++)
      {
        string coinName = coinFullNames[i];
        blacklistedCoins.Add(coinName);
      }
    }
  }
}