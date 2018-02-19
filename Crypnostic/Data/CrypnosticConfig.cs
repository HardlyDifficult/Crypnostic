using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crypnostic
{
  /// <summary>
  /// This configures initial values to use when starting the CrypnosticController.
  /// 
  /// Hint: You could serialize this to/from Json.
  /// </summary>
  [Serializable]
  public class CrypnosticConfig
  {
    #region Data
    /// <summary>
    /// In priority order.
    /// </summary>
    internal readonly ExchangeName[] supportedExchangeList;

    /// <summary>
    /// May not be lowercase.
    /// (better if it's not for Json settings)
    /// </summary>
    internal readonly Dictionary<string, string> coinAliasToName 
      = new Dictionary<string, string>();

    /// <summary>
    /// May not be lowercase.
    /// (better if it's not for Json settings)
    /// </summary>
    internal readonly HashSet<string> blacklistedCoins 
      = new HashSet<string>();
    #endregion

    #region Init
    /// <param name="supportedExchangeList">
    /// List in priority order.  i.e. the first exchange will be considered before others.
    /// Leave blank to support every exchange.
    /// </param>
    public CrypnosticConfig(
      params ExchangeName[] supportedExchangeList)
    {
      if(supportedExchangeList.Length == 0)
      {
        supportedExchangeList = (ExchangeName[])Enum.GetValues(typeof(ExchangeName));
      }

      this.supportedExchangeList = supportedExchangeList;
    }
    #endregion

    #region Public Write API
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
      Debug.Assert(coinFullNames != null);
      Debug.Assert(coinFullNames.Length > 0);

      for (int i = 0; i < coinFullNames.Length; i++)
      {
        string coinName = coinFullNames[i];
        Debug.Assert(string.IsNullOrWhiteSpace(coinName) == false);

        blacklistedCoins.Add(coinName);
      }
    }
    #endregion
  }
}