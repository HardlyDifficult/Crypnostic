using Common.Logging;
using Common.Logging.Configuration;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace Crypnostic.GoogleSheetsExamples
{
  /// <summary>
  /// Getting Started:
  ///  - Follow Steps 1 here to create a client_id.json and place it
  ///  in the Crypnostic.GoogleSheetsExamples\bin directory 
  ///  https://developers.google.com/sheets/api/quickstart/dotnet
  ///  - Follow Steps 2 from the same guide to install the nuget package
  ///  - In GoogleSheetPriceMonitor change the sheet Id to one that you own
  ///  - Make changes to GoogleSheetPriceMonitor's read and write logic for your use case
  /// </summary>
  class Program
  {
    static void Main(
      string[] args)
    {
      // Configure logging once for the application
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



      GoogleSheetPriceMonitor priceMonitor = new GoogleSheetPriceMonitor();
      priceMonitor.Start().Wait();

      while(true)
      {
        if(Console.ReadLine().Equals("Quit", StringComparison.InvariantCultureIgnoreCase))
        {
          return;
        }
      }
    }
  }
}
