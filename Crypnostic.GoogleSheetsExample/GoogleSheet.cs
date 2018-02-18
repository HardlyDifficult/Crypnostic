using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using HD;
using System.Threading.Tasks;

namespace Crypnostic.GoogleSheetsExamples
{
  public class GoogleSheet
  {
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
    static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    const string ApplicationName = "Crypnostic";

    readonly SheetsService service;


    readonly string spreadsheetId;


    /// <summary>
    /// From https://developers.google.com/sheets/api/limits
    /// </summary>
    readonly Throttle readThrottle = new Throttle(TimeSpan.FromSeconds(2*1));
    readonly Throttle writeThrottle = new Throttle(TimeSpan.FromSeconds(2*1));

    public GoogleSheet(
      string spreadsheetId)
    {
      this.spreadsheetId = spreadsheetId;
      UserCredential credential;

      using (FileStream stream =
          new FileStream("..\\client_id.json", FileMode.Open, FileAccess.Read))
      {
        string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        credPath = Path.Combine(credPath,
          ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.Load(stream).Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)).Result;
      }

      // Create Google Sheets API service.
      service = new SheetsService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = ApplicationName,
      });
    }

    public async Task Write(
      string tab,
      string range,
      List<string[]> valueList)
    {
      string writeRange = $"{tab}!{range}";

      ValueRange valueRange = new ValueRange();
      valueRange.Values = valueList.ToArray();
      SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest =
        new SpreadsheetsResource.ValuesResource.UpdateRequest(service,
        valueRange,
        spreadsheetId,
        writeRange);
      updateRequest.ValueInputOption
        = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
      while (true)
      {
        try
        {
          await writeThrottle.WaitTillReady();
          updateRequest.Execute();
          break;
        }
        catch
        {
          await Task.Delay(TimeSpan.FromSeconds(10));
        }
      }
    }

    internal async Task<IList<IList<object>>> Read(
      string tab,
      string range)
    {
      string readRange = $"{tab}!{range}";
      SpreadsheetsResource.ValuesResource.GetRequest request =
        service.Spreadsheets.Values.Get(spreadsheetId, readRange);

      while (true)
      {
        try
        {
          await readThrottle.WaitTillReady();
          ValueRange response = request.Execute();
          return response.Values;
        }
        catch
        {
          await Task.Delay(TimeSpan.FromSeconds(10));
        }
      }
    }
  }
}
