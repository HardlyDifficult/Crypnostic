using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypnostic;
using System;
using System.Collections.Generic;

namespace Crypnostic.Tests.Exchanges
{
  [TestClass()]
  public class BinanceTests : ExchangeMonitorTests
  {
    protected override ExchangeName exchangeName
    {
      get
      {
        return ExchangeName.Binance;
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