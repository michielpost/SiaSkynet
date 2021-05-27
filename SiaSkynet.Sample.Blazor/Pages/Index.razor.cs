using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiaSkynet.Sample.Blazor.Pages
{
    public partial class Index
    {
        SiaSkynetClient client = new SiaSkynetClient();

        string seedPhrase = "";
        string documentText = "";
        string encryptText = "";
        string decryptText = "";
        string loadedDocumentText = "";
        RegistryKey dataKey = new RegistryKey("blazor-sample-3");
        bool saving = false;
        bool loading = false;
        bool success = true;
        byte[]? privateKey;
        byte[]? publicKey;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();

#if RELEASE
            string baseUrl = NavigationManager.BaseUri;
            var uri = new Uri(baseUrl);
            SetPortalDomain(uri.Scheme, uri.Authority);
#endif

        }

        public void SetPortalDomain(string scheme, string domain)
        {
            string[] urlParts = domain.Split('.');

            //Only take last two parts
            var lastParts = urlParts.Skip(urlParts.Count() - 2).Take(2);

            if (lastParts.Count() == 2)
            {
                var url = $"{scheme}://{string.Join('.', lastParts)}";
                Console.WriteLine($"Using API domain: {url}");
                client = new SiaSkynetClient(url);
            }
        }

        private void SetSeedPhraseValue(string Value)
        {
            seedPhrase = Value;
        }

        private void SetDocumentTextValue(string Value)
        {
            documentText = Value;
        }

        private void SetEncryptTextValue(string Value)
        {
            encryptText = Value;
        }

        private void SetDecryptTextValue(string Value)
        {
            decryptText = Value;
        }

        private void InitKeys()
        {
            var key = SiaSkynetClient.GenerateKeys(seedPhrase);
            privateKey = key.privateKey;
            publicKey = key.publicKey;
        }

        private async Task SaveData()
        {
            saving = true;
            success = await client.SkyDbSetAsString(privateKey, publicKey, dataKey, documentText);
            saving = false;

        }

        private async Task LoadData()
        {
            loading = true;
            loadedDocumentText = await client.SkyDbGetAsString(publicKey, dataKey, TimeSpan.FromSeconds(10));
            loading = false;
        }

        private void Encrypt()
        {
            var encrypted = Utils.Encrypt(System.Text.Encoding.UTF8.GetBytes(encryptText), privateKey);
            decryptText = BitConverter.ToString(encrypted).Replace("-", "");

            encryptText = null;
        }

        private void Decrypt()
        {
            var cipherData = Utils.HexStringToByteArray(decryptText);
            var decrypted = Utils.Decrypt(cipherData, privateKey);
            encryptText = System.Text.Encoding.UTF8.GetString(decrypted);

            decryptText = null;
        }
    }
}
