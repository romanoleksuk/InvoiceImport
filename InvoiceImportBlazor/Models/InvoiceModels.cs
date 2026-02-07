namespace InvoiceImportBlazor.Models;

public class InvoiceHeader
{
    public string Vendor { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string Terms { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal? Tax { get; set; }
    public decimal? Freight { get; set; }
}

public class LineItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public decimal UsdAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal CadAmount { get; set; }
    public string GlAccount { get; set; } = string.Empty;
    public string? CostCenter { get; set; }
    public string? Project { get; set; }
    public string? Job { get; set; }
    public string? TaxCode { get; set; }
    public string? Notes { get; set; }
}

public class Invoice
{
    public InvoiceHeader Header { get; set; } = new();
    public List<LineItem> LineItems { get; set; } = new();
}

public class ValidationStatus
{
    public bool TotalsMatch { get; set; }
    public bool MissingCustomFields { get; set; }
    public bool DistributionMatch { get; set; }
}
