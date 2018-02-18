using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypnostic;
using System;

namespace Crypnostic.Tests.Exchanges
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