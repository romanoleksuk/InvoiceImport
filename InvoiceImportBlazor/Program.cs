using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using InvoiceImportBlazor;
using InvoiceImportBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Register application services
builder.Services.AddScoped<AzureDocumentIntelligenceService>();
builder.Services.AddScoped<SampleDataOcrService>();
builder.Services.AddScoped<IOCRService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["Azure:DocumentIntelligence:Endpoint"];
    var apiKey = configuration["Azure:DocumentIntelligence:ApiKey"];
    var isAzureConfigured = IsAzureConfigurationReady(endpoint, apiKey);

    return isAzureConfigured
        ? sp.GetRequiredService<AzureDocumentIntelligenceService>()
        : sp.GetRequiredService<SampleDataOcrService>();
});
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<InvoiceWorkflowState>();

await builder.Build().RunAsync();

static bool IsAzureConfigurationReady(string? endpoint, string? apiKey)
{
    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
    {
        return false;
    }

    return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
