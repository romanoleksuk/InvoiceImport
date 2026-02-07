using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Globalization;
using InvoiceImportBlazor.Models;
using Microsoft.Extensions.Configuration;

namespace InvoiceImportBlazor.Services;

public class AzureDocumentIntelligenceService : IOCRService
{
    private readonly DocumentAnalysisClient? _client;
    private readonly bool _isConfigured;

    public AzureDocumentIntelligenceService(IConfiguration configuration)
    {
        var endpoint = configuration["Azure:DocumentIntelligence:Endpoint"];
        var apiKey = configuration["Azure:DocumentIntelligence:ApiKey"];

        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
        {
            _client = new DocumentAnalysisClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey)
            );
            _isConfigured = true;
        }
        else
        {
            _isConfigured = false;
        }
    }

    public async Task<OCRResult> ProcessInvoiceAsync(byte[] imageData, string contentType)
    {
        if (!_isConfigured || _client == null)
        {
            throw new InvalidOperationException(
                "Azure Document Intelligence is not configured. " +
                "Please add Azure:DocumentIntelligence:Endpoint and ApiKey to appsettings.json"
            );
        }

        using var stream = new MemoryStream(imageData);
        
        // Use prebuilt-invoice model
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-invoice",
            stream
        );

        var result = operation.Value;
        var ocrResult = new OCRResult
        {
            OriginalImageData = imageData,
            ImageContentType = contentType,
            FieldConfidences = new Dictionary<string, FieldConfidence>()
        };

        if (result.Documents.Count > 0)
        {
            var invoice = result.Documents[0];

            // Extract header fields
            ocrResult.Header = ExtractInvoiceHeader(invoice, ocrResult.FieldConfidences);
            
            // Extract line items
            ocrResult.LineItems = ExtractLineItems(invoice, ocrResult.FieldConfidences);

            // Extract currency
            ocrResult.DetectedCurrency = GetFieldValue(invoice, "CurrencyCode") ?? "USD";

            // Extract any handwritten notes (from unstructured content)
            ocrResult.HandwrittenNotes = ExtractHandwrittenNotes(result);
        }

        return ocrResult;
    }

    private InvoiceHeader ExtractInvoiceHeader(AnalyzedDocument invoice, Dictionary<string, FieldConfidence> confidences)
    {
        var header = new InvoiceHeader();

        // Vendor
        var vendorName = GetFieldValue(invoice, "VendorName");
        if (vendorName != null)
        {
            header.Vendor = vendorName;
            AddConfidence(invoice, "VendorName", "Vendor", confidences);
        }

        // Invoice Number
        var invoiceId = GetFieldValue(invoice, "InvoiceId");
        if (invoiceId != null)
        {
            header.InvoiceNumber = invoiceId;
            AddConfidence(invoice, "InvoiceId", "InvoiceNumber", confidences);
        }

        // Invoice Date
        var invoiceDate = GetFieldValue(invoice, "InvoiceDate");
        if (invoiceDate != null)
        {
            header.Date = invoiceDate;
            AddConfidence(invoice, "InvoiceDate", "Date", confidences);
        }

        // Currency
        var currency = GetFieldValue(invoice, "CurrencyCode") ?? "USD";
        header.Currency = currency;
        AddConfidence(invoice, "CurrencyCode", "Currency", confidences);

        // Payment Terms
        var terms = GetFieldValue(invoice, "PaymentTerm");
        if (terms != null)
        {
            header.Terms = terms;
            AddConfidence(invoice, "PaymentTerm", "Terms", confidences);
        }

        // Total Amount
        var totalAmount = GetFieldValue(invoice, "InvoiceTotal");
        if (TryParseOcrDecimal(totalAmount, out var total))
        {
            header.Total = total;
            AddConfidence(invoice, "InvoiceTotal", "Total", confidences);
        }

        // Tax Amount
        var taxAmount = GetFieldValue(invoice, "TotalTax");
        if (TryParseOcrDecimal(taxAmount, out var tax))
        {
            header.Tax = tax;
            AddConfidence(invoice, "TotalTax", "Tax", confidences);
        }

        // Subtotal (calculate freight if possible)
        var subtotal = GetFieldValue(invoice, "SubTotal");
        if (TryParseOcrDecimal(subtotal, out var sub))
        {
            // Freight = Total - Subtotal - Tax
            if (header.Tax.HasValue)
            {
                header.Freight = header.Total - sub - header.Tax.Value;
                if (header.Freight < 0) header.Freight = null;
            }
        }

        return header;
    }

    private List<LineItem> ExtractLineItems(AnalyzedDocument invoice, Dictionary<string, FieldConfidence> confidences)
    {
        var lineItems = new List<LineItem>();

        if (invoice.Fields.TryGetValue("Items", out var itemsField) && itemsField.FieldType == DocumentFieldType.List)
        {
            var items = itemsField.Value.AsList();
            int lineNumber = 1;

            foreach (var item in items)
            {
                if (item.FieldType != DocumentFieldType.Dictionary) continue;

                var fields = item.Value.AsDictionary();
                var lineItem = new LineItem
                {
                    Id = lineNumber.ToString()
                };

                // Description
                if (fields.TryGetValue("Description", out var desc))
                {
                    lineItem.Description = desc.Value.AsString() ?? "";
                    AddFieldConfidence(desc, $"LineItem{lineNumber}_Description", $"Line {lineNumber} Description", confidences);
                }

                // Quantity
                if (fields.TryGetValue("Quantity", out var qty))
                {
                    if (TryParseOcrDecimal(qty.Content, out var quantity))
                    {
                        lineItem.Quantity = quantity;
                    }
                }

                // Amount
                if (fields.TryGetValue("Amount", out var amt))
                {
                    if (TryParseOcrDecimal(amt.Content, out var amount))
                    {
                        lineItem.UsdAmount = amount;
                        lineItem.ExchangeRate = 1.35m; // Default
                        lineItem.CadAmount = amount * lineItem.ExchangeRate;
                        AddFieldConfidence(amt, $"LineItem{lineNumber}_Amount", $"Line {lineNumber} Amount", confidences);
                    }
                }

                // Unit Price
                if (fields.TryGetValue("UnitPrice", out var price))
                {
                    // Store in notes for reference
                    lineItem.Notes = $"Unit Price: {price.Content}";
                }

                lineItems.Add(lineItem);
                lineNumber++;
            }
        }

        return lineItems;
    }

    private static List<string> ExtractHandwrittenNotes(AnalyzeResult result)
    {
        var notes = new List<string>();

        // Look for handwritten styles in the document
        foreach (var page in result.Pages)
        {
            foreach (var line in page.Lines)
            {
                // Check if line appears to be handwritten (Azure marks this in styles)
                if (line.Content.Length > 5) // Ignore very short fragments
                {
                    // In a real implementation, check DocumentStyle for handwriting
                    // For now, we'll skip this as it requires more complex analysis
                }
            }
        }

        return notes;
    }

    private static bool TryParseOcrDecimal(string? rawValue, out decimal value)
    {
        return decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }

    private string? GetFieldValue(AnalyzedDocument document, string fieldName)
    {
        if (document.Fields.TryGetValue(fieldName, out var field))
        {
            return field.Content;
        }
        return null;
    }

    private void AddConfidence(AnalyzedDocument document, string fieldName, string displayName, Dictionary<string, FieldConfidence> confidences)
    {
        if (document.Fields.TryGetValue(fieldName, out var field))
        {
            AddFieldConfidence(field, fieldName, displayName, confidences);
        }
    }

    private void AddFieldConfidence(DocumentField field, string key, string displayName, Dictionary<string, FieldConfidence> confidences)
    {
        var confidence = field.Confidence ?? 0;
        confidences[key] = new FieldConfidence
        {
            FieldName = displayName,
            Score = confidence * 100 // Convert to percentage
        };
    }
}
