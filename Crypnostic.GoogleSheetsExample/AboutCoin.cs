using System;

namespace Crypnostic.GoogleSheetsExamples
{
  internal class AboutCoin
  {
    public enum Column
    {
      CoinName,
      BestBidBTC,
      BestBidBTCUSD,
      BestBidBTCExchange,
      BestAskBTC,
      BestAskBTCUSD,
      BestAskBTCExchange,
      BestBidETH,
      BestBidETHUSD,
      BestBidETHExchange,
      BestAskETH,
      BestAskETHUSD,
      BestAskETHExchange,
      BestBidUSD,
      BestBidUSDExchange,
      BestAskUSD,
      BestAskUSDExchange,
      MarketCapUSD,
      TotalVolumeSeen
    }

    public readonly string[] columns;

    public AboutCoin()
    {
      columns = new string[Enum.GetValues(typeof(Column)).Length];
      for (int i = 0; i < columns.Length; i++)
      {
        columns[i] = "";
      }
    }

    public void PopulateWithColumnNames()
    {
      for (int i = 0; i < columns.Length; i++)
      {
        columns[i] = ((Column)i).ToString();
      }
    }

    public string[] ToArray()
    {
      return columns;
    }
  }
}