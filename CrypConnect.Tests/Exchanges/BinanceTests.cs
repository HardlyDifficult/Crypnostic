using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypConnect;
using System;
using System.Collections.Generic;

namespace CrypConnect.Tests.Exchanges
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