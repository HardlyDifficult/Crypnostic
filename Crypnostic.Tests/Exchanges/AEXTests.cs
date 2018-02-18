using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypnostic;
using System;
using System.Threading.Tasks;

namespace Crypnostic.Tests.Exchanges
{
  [TestClass()]
  public class AEXTests : ExchangeMonitorTests
  {
    protected override ExchangeName exchangeName
    {
      get
      {
        return ExchangeName.AEX;
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
        return Coin.FromName("Ardor");
      }
    } 
  }
}