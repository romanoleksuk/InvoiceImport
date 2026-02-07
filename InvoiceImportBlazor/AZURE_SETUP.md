# Azure Document Intelligence Configuration

To enable real OCR processing, you need to create an Azure Document Intelligence resource:

## Step 1: Create Azure Resource

1. Go to [Azure Portal](https://portal.azure.com)
2. Click "+ Create a resource"
3. Search for "Document Intelligence" (or "Form Recognizer")
4. Click "Create"
5. Fill in:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Create new or use existing
   - **Region**: Choose closest to you (e.g., East US, West Europe)
   - **Name**: Choose a unique name (e.g., invoice-ocr-yourname)
   - **Pricing Tier**: **Free F0** (500 pages/month free) or **S0** ($1.50/1000 pages)

6. Click "Review + Create" → "Create"
7. Wait 1-2 minutes for deployment

## Step 2: Get Your Credentials

1. Go to your new resource
2. Click "Keys and Endpoint" in the left menu
3. Copy:
   - **Endpoint**: e.g., `https://invoice-ocr-yourname.cognitiveservices.azure.com/`
   - **KEY 1**: e.g., `abc123def456...`

## Step 3: Configure the Application

### Option A: Update appsettings.json (Quick Test)
Edit `wwwroot/appsettings.json`:
```json
{
  "Azure": {
    "DocumentIntelligence": {
      "Endpoint": "https://YOUR-RESOURCE-NAME.cognitiveservices.azure.com/",
      "ApiKey": "YOUR-KEY-HERE"
    }
  }
}
```

⚠️ **Warning**: Don't commit API keys to source control!

### Option B: User Secrets (Production)
```powershell
dotnet user-secrets init
dotnet user-secrets set "Azure:DocumentIntelligence:Endpoint" "https://your-endpoint/"
dotnet user-secrets set "Azure:DocumentIntelligence:ApiKey" "your-key"
```

### Option C: Environment Variables (Deployment)
```powershell
$env:Azure__DocumentIntelligence__Endpoint = "https://your-endpoint/"
$env:Azure__DocumentIntelligence__ApiKey = "your-key"
```

## Step 4: OCR Provider

`Program.cs` uses a DI factory for `IOCRService`:
```csharp
builder.Services.AddScoped<IOCRService>(sp => /* Azure or SampleData based on config */);
```

If Azure credentials are missing, DI automatically resolves `SampleDataOcrService` for demo usage.

## Testing

1. Upload a test invoice (PDF or image)
2. Azure will extract:
   - Vendor name
   - Invoice number, date
   - Line items with descriptions and amounts
   - Tax, subtotal, total
   - Confidence scores for each field

## Free Tier Limits

- **500 pages/month free**
- Resets monthly
- After 500: $1.50 per 1,000 pages
- No credit card required for free tier

## Troubleshooting

**"Azure Document Intelligence is not configured" error:**
- Check appsettings.json has correct endpoint and key
- Verify endpoint format: must end with `/`
- Ensure API key is correct (copy KEY 1 from Azure portal)

**"Unauthorized" error:**
- API key may be wrong
- Resource might be in different subscription
- Try regenerating KEY 2 and using that

**"Resource not found":**
- Endpoint URL might be wrong
- Check resource is in same region
