# Free Deployment Guide for Invoice Import Blazor App

## Option 1: Azure Static Web Apps (Recommended) ⭐

**Why Azure Static Web Apps?**
- ✅ Free tier with custom domains
- ✅ Automatic CI/CD from GitHub
- ✅ Perfect for Blazor WebAssembly
- ✅ Integrates seamlessly with your Azure Document Intelligence Service
- ✅ Global CDN included

### Prerequisites
- GitHub account (free)
- Azure account (free tier available)

### Deployment Steps

#### 1. Push Your Code to GitHub
```powershell
# Initialize git if not already done
git init
git add .
git commit -m "Initial commit"

# Create a new repository on GitHub, then:
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
git branch -M main
git push -u origin main
```

#### 2. Create Azure Static Web App
1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → Search for "Static Web App"
3. Click "Create"
4. Fill in the details:
   - **Subscription**: Your subscription
   - **Resource Group**: Create new or use existing
   - **Name**: invoice-import-app (or your preferred name)
   - **Plan type**: Free
   - **Region**: Choose closest to you
   - **Source**: GitHub
   - **GitHub Account**: Sign in and authorize
   - **Organization**: Your GitHub username
   - **Repository**: Select your repository
   - **Branch**: main
5. Build Details:
   - **Build Presets**: Blazor
   - **App location**: /InvoiceImportBlazor
   - **Api location**: (leave empty)
   - **Output location**: wwwroot
6. Click "Review + Create" → "Create"

#### 3. Configure Azure Credentials
After deployment, you need to add your Azure Document Intelligence credentials:

1. In Azure Portal, go to your Static Web App
2. Click "Configuration" in the left menu
3. Add these Application Settings:
   - Name: `Azure__DocumentIntelligence__Endpoint`
   - Value: Your Azure Document Intelligence endpoint
   - Name: `Azure__DocumentIntelligence__ApiKey`
   - Value: Your Azure Document Intelligence API key

4. Click "Save"

#### 4. Access Your App
- Your app will be available at: `https://YOUR_APP_NAME.azurestaticapps.net`
- Every push to main branch will automatically redeploy your app

---

## Option 2: GitHub Pages

**Pros**: Completely free, easy setup
**Cons**: Requires manual deployment, custom domain needs paid GitHub plan

### Deployment Steps

#### 1. Add GitHub Actions Workflow
The workflow file has been created at `.github/workflows/deploy-gh-pages.yml`

#### 2. Enable GitHub Pages
1. Go to your GitHub repository → Settings → Pages
2. Source: Deploy from a branch
3. Branch: gh-pages, /root
4. Click Save

#### 3. Configure Base Path
The app needs to know its base path. Update your `index.html` if needed.

#### 4. Push and Deploy
```powershell
git add .
git commit -m "Add GitHub Pages deployment"
git push
```

Your app will be available at: `https://YOUR_USERNAME.github.io/YOUR_REPO_NAME/`

---

## Option 3: Netlify

### Quick Deploy via Netlify CLI

#### 1. Install Netlify CLI
```powershell
npm install -g netlify-cli
```

#### 2. Build Your App
```powershell
cd InvoiceImportBlazor
dotnet publish -c Release
```

#### 3. Deploy
```powershell
netlify deploy --dir=bin/Release/net10.0/publish/wwwroot --prod
```

#### 4. Set Environment Variables
In Netlify dashboard:
- Site settings → Build & deploy → Environment
- Add your Azure credentials:
  - `Azure__DocumentIntelligence__Endpoint`
  - `Azure__DocumentIntelligence__ApiKey`

---

## Important Notes

### Security Considerations
⚠️ **Never commit Azure API keys to GitHub!**
- Use environment variables or Azure Key Vault
- Add `appsettings.json` to `.gitignore` if it contains secrets
- For production, consider using Azure Managed Identity

### appsettings.json Configuration
Your `wwwroot/appsettings.json` should have placeholders:
```json
{
  "Azure": {
    "DocumentIntelligence": {
      "Endpoint": "",
      "ApiKey": ""
    }
  }
}
```

Configure actual values in your hosting platform's environment variables.

### CORS Configuration
If you encounter CORS issues with Azure Document Intelligence:
1. Go to Azure Portal → Your Document Intelligence resource
2. Navigate to "CORS" settings
3. Add your deployed app URL to allowed origins

---

## Recommended Choice

For your use case, I recommend **Azure Static Web Apps** because:
1. It's free
2. Seamless integration with Azure Document Intelligence
3. Automatic deployments from GitHub
4. Professional SSL certificates
5. Custom domains included
6. No CORS configuration needed (same Azure environment)

Let me know which option you'd like to pursue, and I can help with any specific configuration!
