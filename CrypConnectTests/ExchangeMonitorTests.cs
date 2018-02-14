using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchanges.Tests
{
  public abstract class ExchangeMonitorTests
  {
    protected ExchangeMonitor monitor;

    [TestCleanup]
    public void Cleanup()
    {
      monitor.Stop();
      monitor = null;
    }
  }
}
