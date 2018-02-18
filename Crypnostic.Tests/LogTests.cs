using Common.Logging;
using Common.Logging.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Config;
using NLog.Targets;

namespace Crypnostic.Tests
{
  [TestClass()]
  public class LogTests //: MonitorTests
  {
    [TestMethod()]
    public void BasicLog()
    {
      // Run once for the application
      FileTarget target = new FileTarget(
        "Something, Anything goes here... and does... nothing?")
      {
        FileName = "Log.txt"
      };

      LoggingConfiguration loggingConfig = new LoggingConfiguration();
      loggingConfig.AddTarget(target);
      loggingConfig.AddRuleForAllLevels(target, "*");
      NLog.LogManager.Configuration = loggingConfig;
      LogManager.Adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter(
        new NameValueCollection());

      // Run once per class
      ILog log = LogManager.GetLogger<LogTests>();

      // Each log statement
      log.Fatal("Such Fatal, Much Wow");
    }
  }
}