using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CrypConnect.Tests
{
  public class MonitorTests
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