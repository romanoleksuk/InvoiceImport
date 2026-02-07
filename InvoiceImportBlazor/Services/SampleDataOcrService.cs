using InvoiceImportBlazor.Data;
using InvoiceImportBlazor.Models;

namespace InvoiceImportBlazor.Services;

public class SampleDataOcrService : IOCRService
{
    public Task<OCRResult> ProcessInvoiceAsync(byte[] imageData, string contentType)
    {
        var sample = SampleData.GetSampleOCRResult(imageData, contentType);
        return Task.FromResult(sample);
    }
}
