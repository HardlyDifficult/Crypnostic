using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypnostic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crypnostic.Tests.Exchanges
{
  [TestClass()]
  public class KucoinTests : ExchangeMonitorTests
  {
    protected override ExchangeName exchangeName
    {
      get
      {
        return ExchangeName.Kucoin;
      }
    }

    protected override Coin popularBaseCoin
    {
      get
      {
        return Coin.bitcoin; 
      }
    } 

    protected override Coin popularQuoteCoin
    {
      get
      {
        return Coin.ethereum; 
      }
    } 
  }
}