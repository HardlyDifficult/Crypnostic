using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crypnostic.Tests
{
  public class MonitorTests
  {
    protected CrypnosticController monitor;

    [TestCleanup]
    public void Cleanup()
    {
      monitor.Stop();
      monitor = null;
    }
  }
}