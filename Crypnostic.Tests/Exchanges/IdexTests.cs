using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;

namespace CrypConnect.Tests.Exchanges
{
  [TestClass()]
  public class IdexTests : ExchangeMonitorTests
  {
    protected override ExchangeName exchangeName
    {
      get
      {
        return ExchangeName.Idex;
      }
    }

    protected override Coin popularBaseCoin
    {
      get
      {
        return Coin.ethereum;
      }
    } 

    protected override Coin popularQuoteCoin
    {
      get
      {
        return Coin.FromName("Polymath"); 
      }
    } 
  }
}