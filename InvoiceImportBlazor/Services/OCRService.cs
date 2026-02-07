using InvoiceImportBlazor.Models;

namespace InvoiceImportBlazor.Services;

public interface IOCRService
{
    Task<OCRResult> ProcessInvoiceAsync(byte[] imageData, string contentType);
}
