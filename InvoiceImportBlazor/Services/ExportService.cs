using System.Text;
using System.Text.Json;
using System.Globalization;
using InvoiceImportBlazor.Models;

namespace InvoiceImportBlazor.Services;

public interface IExportService
{
    byte[] ExportToJson(Invoice invoice, OCRResult? ocrResult = null);
    byte[] ExportToCsv(Invoice invoice);
    string GetFileName(string format, string invoiceNumber);
}

public class ExportService : IExportService
{
    public byte[] ExportToJson(Invoice invoice, OCRResult? ocrResult = null)
    {
        var exportData = new
        {
            invoice.Header,
            invoice.LineItems,
            OCRMetadata = ocrResult != null ? new
            {
                ocrResult.FieldConfidences,
                ocrResult.HandwrittenNotes,
                ocrResult.DetectedCurrency
            } : null,
            ExportedAt = DateTime.UtcNow,
            ExportVersion = "1.0"
        };

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Encoding.UTF8.GetBytes(json);
    }

    public byte[] ExportToCsv(Invoice invoice)
    {
        var csv = new StringBuilder();

        // Header row
        AppendCsvRow(csv,
        [
            "LineNumber", "Description", "Quantity", "USDAmount", "ExchangeRate",
            "CADAmount", "GLAccount", "CostCenter", "Project", "Job", "TaxCode", "Notes"
        ]);

        // Data rows
        int lineNumber = 1;
        foreach (var item in invoice.LineItems)
        {
            AppendCsvRow(csv,
            [
                lineNumber.ToString(CultureInfo.InvariantCulture),
                item.Description,
                item.Quantity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                item.UsdAmount.ToString("F2", CultureInfo.InvariantCulture),
                item.ExchangeRate.ToString("F4", CultureInfo.InvariantCulture),
                item.CadAmount.ToString("F2", CultureInfo.InvariantCulture),
                item.GlAccount,
                item.CostCenter ?? string.Empty,
                item.Project ?? string.Empty,
                item.Job ?? string.Empty,
                item.TaxCode ?? string.Empty,
                item.Notes ?? string.Empty
            ]);

            lineNumber++;
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public string GetFileName(string format, string invoiceNumber)
    {
        var safeInvoiceNumber = string.Join("_", invoiceNumber.Split(Path.GetInvalidFileNameChars()));
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"Invoice_{safeInvoiceNumber}_{timestamp}.{format}";
    }

    private static void AppendCsvRow(StringBuilder csv, IEnumerable<string> fields)
    {
        csv.AppendLine(string.Join(",", fields.Select(field => EscapeCsvStatic(field))));
    }

    private static string EscapeCsvStatic(string value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
