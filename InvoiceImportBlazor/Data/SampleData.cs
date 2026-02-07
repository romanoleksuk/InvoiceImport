using InvoiceImportBlazor.Models;

namespace InvoiceImportBlazor.Data;

public static class SampleData
{
    public static Invoice GetSampleInvoice()
    {
        return new Invoice
        {
            Header = new InvoiceHeader
            {
                Vendor = "Acme Corp",
                InvoiceNumber = "INV-2024-001",
                Date = "2024-01-15",
                Currency = "USD",
                Terms = "Net 30",
                Total = 5250.00m,
                Tax = 525.00m,
                Freight = 150.00m
            },
            LineItems = new List<LineItem>
            {
                new LineItem
                {
                    Id = "1",
                    Description = "Professional Services - Consulting",
                    Quantity = 40,
                    UsdAmount = 4000.00m,
                    ExchangeRate = 1.35m,
                    CadAmount = 5400.00m,
                    GlAccount = "5000-100",
                    CostCenter = "CC-001",
                    Project = "PRJ-2024-A",
                    TaxCode = "HST",
                    Notes = "Q1 consulting work"
                },
                new LineItem
                {
                    Id = "2",
                    Description = "Software License - Annual",
                    Quantity = 1,
                    UsdAmount = 1000.00m,
                    ExchangeRate = 1.35m,
                    CadAmount = 1350.00m,
                    GlAccount = "5200-150",
                    CostCenter = "CC-002",
                    TaxCode = "HST",
                    Notes = ""
                },
                new LineItem
                {
                    Id = "3",
                    Description = "Shipping & Handling",
                    UsdAmount = 250.00m,
                    ExchangeRate = 1.35m,
                    CadAmount = 337.50m,
                    GlAccount = "5300-200",
                    CostCenter = "CC-001",
                    TaxCode = "EXEMPT",
                    Notes = "Express shipping"
                }
            }
        };
    }

    public static OCRResult GetSampleOCRResult(byte[]? imageData = null, string? contentType = null)
    {
        var invoice = GetSampleInvoice();
        
        return new OCRResult
        {
            OriginalImageData = imageData,
            ImageContentType = contentType ?? "sample",
            DetectedCurrency = "USD",
            Header = invoice.Header,
            LineItems = invoice.LineItems,
            HandwrittenNotes = new List<string>
            {
                "Approved by Manager",
                "Priority: Standard"
            },
            FieldConfidences = new Dictionary<string, FieldConfidence>
            {
                { "Vendor", new FieldConfidence { FieldName = "Vendor", Score = 98.5 } },
                { "InvoiceNumber", new FieldConfidence { FieldName = "InvoiceNumber", Score = 99.2 } },
                { "Date", new FieldConfidence { FieldName = "Date", Score = 97.1 } },
                { "Currency", new FieldConfidence { FieldName = "Currency", Score = 95.8 } },
                { "Total", new FieldConfidence { FieldName = "Total", Score = 96.3 } },
                { "Tax", new FieldConfidence { FieldName = "Tax", Score = 94.7 } },
                { "Freight", new FieldConfidence { FieldName = "Freight", Score = 93.4 } }
            }
        };
    }
}
