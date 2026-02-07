using InvoiceImportBlazor.Models;

namespace InvoiceImportBlazor.Services;

public interface IValidationService
{
    ValidationResult ValidateInvoice(Invoice invoice, ValidationSettings settings);
}

public class ValidationService : IValidationService
{
    public ValidationResult ValidateInvoice(Invoice invoice, ValidationSettings settings)
    {
        var result = new ValidationResult
        {
            Issues = new List<ValidationIssue>()
        };

        // Check required fields
        CheckRequiredFields(invoice, settings, result);

        // Validate totals match within tolerance
        ValidateTotals(invoice, settings, result);
        ValidateCadDistribution(invoice, settings, result);

        // Check currency detection
        if (settings.RequireCurrencyDetection && string.IsNullOrWhiteSpace(invoice.Header.Currency))
        {
            result.Issues.Add(new ValidationIssue
            {
                Field = "Currency",
                Message = "Currency not detected",
                Severity = ValidationSeverity.Error
            });
        }

        // Check for missing GL accounts or cost centers
        var missingGLOrCC = invoice.LineItems.Any(i => 
            string.IsNullOrWhiteSpace(i.GlAccount) || 
            string.IsNullOrWhiteSpace(i.CostCenter));

        if (missingGLOrCC)
        {
            result.Issues.Add(new ValidationIssue
            {
                Field = "LineItems",
                Message = "Some line items are missing GL Account or Cost Center",
                Severity = ValidationSeverity.Warning
            });
        }

        // Calculate validation status
        result.Status = new ValidationStatus
        {
            TotalsMatch = !result.Issues.Any(i => i.Field == "Total" && i.Severity == ValidationSeverity.Error),
            MissingCustomFields = missingGLOrCC,
            DistributionMatch = !result.Issues.Any(i => i.Field == "Distribution" && i.Severity == ValidationSeverity.Error)
        };

        result.IsValid = !result.Issues.Any(i => i.Severity == ValidationSeverity.Error);

        return result;
    }

    private void CheckRequiredFields(Invoice invoice, ValidationSettings settings, ValidationResult result)
    {
        foreach (var field in settings.RequiredFields)
        {
            var isEmpty = field switch
            {
                "Vendor" => string.IsNullOrWhiteSpace(invoice.Header.Vendor),
                "InvoiceNumber" => string.IsNullOrWhiteSpace(invoice.Header.InvoiceNumber),
                "Date" => string.IsNullOrWhiteSpace(invoice.Header.Date),
                "Total" => invoice.Header.Total == 0,
                _ => false
            };

            if (isEmpty)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Field = field,
                    Message = $"{field} is required",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }

    private void ValidateTotals(Invoice invoice, ValidationSettings settings, ValidationResult result)
    {
        var lineItemsTotal = invoice.LineItems.Sum(i => i.UsdAmount);
        var headerTotal = invoice.Header.Total;
        var difference = Math.Abs(headerTotal - lineItemsTotal);

        if (difference > settings.TotalTolerance)
        {
            result.Issues.Add(new ValidationIssue
            {
                Field = "Total",
                Message = $"Line items total (${lineItemsTotal:N2}) does not match header total (${headerTotal:N2}). Difference: ${difference:N2}",
                Severity = ValidationSeverity.Error
            });
        }
        else if (difference > 0)
        {
            result.Issues.Add(new ValidationIssue
            {
                Field = "Total",
                Message = $"Minor rounding difference detected: ${difference:N2}",
                Severity = ValidationSeverity.Info
            });
        }
    }

    private void ValidateCadDistribution(Invoice invoice, ValidationSettings settings, ValidationResult result)
    {
        if (!invoice.LineItems.Any())
        {
            return;
        }

        var cadDistributionTotal = invoice.LineItems.Sum(i => i.CadAmount);
        var activeRate = invoice.LineItems.FirstOrDefault(i => i.ExchangeRate > 0m)?.ExchangeRate ?? 1m;
        var expectedCadTotal = invoice.Header.Total * activeRate;
        var difference = Math.Abs(expectedCadTotal - cadDistributionTotal);

        if (difference > settings.TotalTolerance)
        {
            result.Issues.Add(new ValidationIssue
            {
                Field = "Distribution",
                Message = $"CAD distribution total (${cadDistributionTotal:N2}) does not match expected CAD total (${expectedCadTotal:N2}). Difference: ${difference:N2}",
                Severity = ValidationSeverity.Error
            });
        }
    }
}
