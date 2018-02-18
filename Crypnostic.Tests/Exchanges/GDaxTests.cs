using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypConnect.Tests.Exchanges
{
  [TestClass()]
  public class GDaxTests : ExchangeMonitorTests
  {
    protected override ExchangeName exchangeName
    {
      get
      {
        return ExchangeName.GDax;
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