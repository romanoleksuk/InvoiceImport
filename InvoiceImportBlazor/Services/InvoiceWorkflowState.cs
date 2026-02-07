using InvoiceImportBlazor.Models;

namespace InvoiceImportBlazor.Services;

public class InvoiceWorkflowState
{
    private readonly IValidationService _validationService;

    public InvoiceWorkflowState(IValidationService validationService)
    {
        _validationService = validationService;
    }

    public string CurrentView { get; private set; } = "import";
    public Invoice? Invoice { get; private set; }
    public OCRResult? OcrResult { get; private set; }
    public ValidationResult? ValidationResult { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1.35m;
    public bool IsRateLocked { get; private set; }
    public ValidationSettings ValidationSettings { get; } = new()
    {
        TotalTolerance = 0.02m,
        RequiredFields = new List<string> { "Vendor", "InvoiceNumber", "Date", "Total" },
        RequireCurrencyDetection = true
    };

    public void LoadFromOcrResult(OCRResult ocrResult)
    {
        OcrResult = ocrResult;
        Invoice = new Invoice
        {
            Header = ocrResult.Header,
            LineItems = ocrResult.LineItems
        };

        ExchangeRate = ocrResult.DetectedCurrency == "CAD"
            ? 1.0m
            : GetPreferredExchangeRate(ocrResult.LineItems, ExchangeRate);

        ApplyExchangeRateToAllLineItems(ExchangeRate);
        Revalidate();
        CurrentView = "workflow";
    }

    public void SwitchToImportView()
    {
        CurrentView = "import";
    }

    public void ToggleRateLock()
    {
        IsRateLocked = !IsRateLocked;
    }

    public void UpdateExchangeRate(decimal newRate)
    {
        if (IsRateLocked || Invoice == null || newRate <= 0)
        {
            return;
        }

        ExchangeRate = newRate;
        ApplyExchangeRateToAllLineItems(newRate);
        Revalidate();
    }

    public void UpdateLineItems(List<LineItem> lineItems, bool syncRateFromLineItems = false)
    {
        if (Invoice == null)
        {
            return;
        }

        Invoice.LineItems = lineItems;

        if (!IsRateLocked && syncRateFromLineItems && lineItems.Count > 0)
        {
            var rate = lineItems[0].ExchangeRate;
            if (rate > 0)
            {
                ExchangeRate = rate;
            }
        }

        Revalidate();
    }

    public decimal GetCadDistributionTotal()
    {
        return Invoice?.LineItems.Sum(i => i.CadAmount) ?? 0m;
    }

    private void ApplyExchangeRateToAllLineItems(decimal rate)
    {
        if (Invoice == null)
        {
            return;
        }

        foreach (var item in Invoice.LineItems)
        {
            item.ExchangeRate = rate;
            item.CadAmount = item.UsdAmount * rate;
        }
    }

    private void Revalidate()
    {
        if (Invoice == null)
        {
            ValidationResult = null;
            return;
        }

        ValidationResult = _validationService.ValidateInvoice(Invoice, ValidationSettings);
    }

    private static decimal GetPreferredExchangeRate(List<LineItem> lineItems, decimal fallback)
    {
        var firstRate = lineItems.FirstOrDefault(i => i.ExchangeRate > 0m)?.ExchangeRate;
        return firstRate ?? fallback;
    }
}
