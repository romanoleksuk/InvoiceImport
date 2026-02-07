namespace InvoiceImportBlazor.Models;

public enum ConfidenceLevel
{
    High,    // >= 90%
    Medium,  // 70-89%
    Low      // < 70%
}

public class FieldConfidence
{
    public string FieldName { get; set; } = string.Empty;
    public double Score { get; set; } // 0-100
    public ConfidenceLevel Level => Score >= 90 ? ConfidenceLevel.High : 
                                     Score >= 70 ? ConfidenceLevel.Medium : 
                                     ConfidenceLevel.Low;
    public bool IsLowConfidence => Level == ConfidenceLevel.Low;
}

public class OCRResult
{
    public InvoiceHeader Header { get; set; } = new();
    public List<LineItem> LineItems { get; set; } = new();
    public Dictionary<string, FieldConfidence> FieldConfidences { get; set; } = new();
    public string? DetectedCurrency { get; set; }
    public List<string> HandwrittenNotes { get; set; } = new();
    public byte[]? OriginalImageData { get; set; }
    public string? ImageContentType { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public ValidationStatus Status { get; set; } = new();
}

public class ValidationIssue
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; }
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}

public static class ExportFormat
{
    public const string JSON = "json";
    public const string CSV = "csv";
}

public class ValidationSettings
{
    public decimal TotalTolerance { get; set; } = 0.02m; // 2 cents
    public List<string> RequiredFields { get; set; } = new() 
    { 
        "Vendor", 
        "InvoiceNumber", 
        "Date", 
        "Total" 
    };
    public bool RequireCurrencyDetection { get; set; } = true;
}
