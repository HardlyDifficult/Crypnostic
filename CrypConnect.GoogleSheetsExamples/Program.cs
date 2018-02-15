using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CrypConnect.GoogleSheetsExamples
{
  /// <summary>
  /// Getting Started:
  ///  - Follow Steps 1 here to create a client_id.json and place it
  ///  in the CrypConnect.GoogleSheetsExamples\bin directory 
  ///  https://developers.google.com/sheets/api/quickstart/dotnet
  ///  - Follow Steps 2 from the same guide to install the nuget package
  /// </summary>
  class Program
  {
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
    static string[] Scopes = { SheetsService.Scope.Spreadsheets };
    static string ApplicationName = "CrypConnect";

    static void Main(
      string[] args)
    {
      UserCredential credential;

      using (var stream =
          new FileStream("..\\client_id.json", FileMode.Open, FileAccess.Read))
      {
        string credPath = Environment.GetFolderPath(
            Environment.SpecialFolder.Personal);
        credPath = Path.Combine(credPath, 
          ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.Load(stream).Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)).Result;
        Console.WriteLine("Credential file saved to: " + credPath);
      }

      // Create Google Sheets API service.
      var service = new SheetsService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = ApplicationName,
      });

      // Define request parameters.
      String spreadsheetId = "1RoFMncCxV4ExqFQCRKSOmo-7WBSGc7F-9HjLc-5OT2c";



      string writeRange = "CrypConnect!A1";
      ValueRange valueRange = new ValueRange();
      valueRange.Values  = new List<IList<object>>() {
        new List<object>() { "Test1", "Test2" } };
      SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest =
        new SpreadsheetsResource.ValuesResource.UpdateRequest(service,
        valueRange,
        spreadsheetId,
        writeRange);
      updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
      updateRequest.Execute();



      string range = "CrypConnect!A1:E";
      SpreadsheetsResource.ValuesResource.GetRequest request =
              service.Spreadsheets.Values.Get(spreadsheetId, range);

      // Prints the names and majors of students in a sample spreadsheet:
      // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
      ValueRange response = request.Execute();
      IList<IList<Object>> values = response.Values;
      if (values != null && values.Count > 0)
      {
        foreach (var row in values)
        {
          // Print columns A and E, which correspond to indices 0 and 4.
          Console.WriteLine("{0}", row[0]);
        }
      }
      else
      {
        Console.WriteLine("No data found.");
      }
      Console.Read();


    }
  }
}
