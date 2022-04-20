using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Bidder.Activities.Services
{
    public interface ISecurityKeyRepository
    {
        SymmetricSecurityKey SecurityKey { get; }
    }

    public class SecurityKeyRepository : ISecurityKeyRepository
    {
        private static string _keyVaultUri;

        private SymmetricSecurityKey _securityKey;
        public SymmetricSecurityKey SecurityKey
        {
            get
            {
                if (_securityKey == null)
                {
                    _securityKey = GetSecurityKey();
                }
                return _securityKey;
            }
        }

        public SecurityKeyRepository(SecurityKeySettings securityKeySettings)
        {
            _keyVaultUri = securityKeySettings.KeyVaultUri;
        }

        private static SymmetricSecurityKey GetSecurityKey()
        {
            var azureCred = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ManagedIdentityClientId = Environment.GetEnvironmentVariable("AppObjectId")
            });

            var client = new KeyClient(vaultUri: new Uri(_keyVaultUri), credential: azureCred);
            var key = client.GetKey("CustomerToken-Api-SigningKey");
            var rsa = key.Value.Key.ToRSA();
            var exportRsaPublicKey = rsa.ExportRSAPublicKey();
            var symmetricSecurityKey = new SymmetricSecurityKey(exportRsaPublicKey);
            return symmetricSecurityKey;
        }
    }

    public class SecurityKeySettings
    {
        public string KeyVaultUri { get; set; }
    }
}
