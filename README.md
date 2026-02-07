# Invoice Import (Blazor WebAssembly)

Single-page invoice import workflow built with Blazor WebAssembly.

## Stack
- .NET 10 Blazor WebAssembly
- Azure Document Intelligence SDK (optional OCR)

## Run
```powershell
$env:DOTNET_CLI_HOME = "$PWD\\.dotnet"
dotnet restore
dotnet run --project InvoiceImportBlazor
```

## OCR Providers
`Program.cs` uses a DI factory for `IOCRService`:
- `AzureDocumentIntelligenceService`, if both `Endpoint` and `ApiKey` are configured.
- `SampleDataOcrService`, if Azure config is missing.

## Configuration
Set Azure settings in `InvoiceImportBlazor/wwwroot/appsettings.json`:

```json
{
  "Azure": {
    "DocumentIntelligence": {
      "Endpoint": "https://<resource>.cognitiveservices.azure.com/",
      "ApiKey": "<key>"
    }
  }
}
```

Do not commit real credentials.
