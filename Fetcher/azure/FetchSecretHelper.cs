using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsom_bot.Fetcher.azure
{
    public static class FetchSecretHelper
    {
        public static async Task<string> FetchSecret(string key)
        {
            // Define your Azure AD credentials
            string tenantId = "<your-tenant-id>";       // Azure AD Tenant ID
            string clientId = "<your-client-id>";       // Azure AD App Registration's Client ID
            string clientSecret = "<your-client-secret>"; // Client Secret for the App Registration

            // Configure DefaultAzureCredentialOptions
            var options = new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = false, // Include environment variables
                ExcludeManagedIdentityCredential = true, // Exclude Managed Identity
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeAzureCliCredential = true,
                ExcludeInteractiveBrowserCredential = true
            };

            // Set environment variables for authentication
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", clientId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", clientSecret);
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", tenantId);

            //Key Vault and Secrets
            string keyVaultUrl = "https://death-star-bot.vault.azure.net/secrets";

            //Create SectretClient instance
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            //Retrieve the secret value
            KeyVaultSecret secret = await client.GetSecretAsync(key);
            string secretValue = secret.Value;

            return secretValue;
        }
    }
}
