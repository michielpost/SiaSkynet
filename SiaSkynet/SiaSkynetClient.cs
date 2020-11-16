using Ed25519;
using Isopoh.Cryptography.Blake2b;
using Isopoh.Cryptography.SecureArray;
using MimeTypes;
using Newtonsoft.Json;
using P3.Ed25519.HdKey;
using RestEase;
using SiaSkynet.Requests;
using SiaSkynet.Responses;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiaSkynet
{
    public class SiaSkynetClient
    {
        private const string apiBaseUrl = "https://siasky.net/";

        private ISiaSkynetApi _api;

        public SiaSkynetClient(string baseUrl = apiBaseUrl)
        {
            _api = GetApi(baseUrl);

        }

        public ISiaSkynetApi GetApi(string baseUrl = apiBaseUrl)
        {
            var nbApi = new RestClient(baseUrl)
            {

                //JsonSerializerSettings = new JsonSerializerSettings()
                //{
                //    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                //    NullValueHandling = NullValueHandling.Ignore,
                //    Converters = new List<JsonConverter> { new StringEnumConverter() }
                //}
            }.For<ISiaSkynetApi>();
            return nbApi;
        }

        public Task<SkyfileResponse> UploadFileAsync(string fileName, Stream file)
        {
            string extensions = Path.GetExtension(fileName);
            var contentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(extensions));
            return _api.UploadFile(contentType, fileName, file);
        }

        public Task<Stream> DownloadFileAsStreamAsync(string skylink)
        {
            return _api.GetFileAsStream(skylink);
        }

        public async Task<(string file, string? contentType, SkynetFileMetadata metadata)> DownloadFileAsStringAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                string file = await httpResult.Content.ReadAsStringAsync();

                ReadFileHeaders(httpResult, out string? contentType, out SkynetFileMetadata metadata);

                return (file, contentType, metadata);
            }
        }

        public async Task<(byte[] file, string? contentType, SkynetFileMetadata metadata)> DownloadFileAsByteArrayAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                byte[] file = await httpResult.Content.ReadAsByteArrayAsync();

                ReadFileHeaders(httpResult, out string? contentType, out SkynetFileMetadata metadata);

                return (file, contentType, metadata);
            }
        }

        public static async Task<(byte[] privateKey, byte[] publicKey)> GenerateKeys(string seed)
        {
            var privateKey = Ed25519HdKey.GetMasterKeyFromSeed(Encoding.UTF8.GetBytes(seed));
            var publicKey = Ed25519HdKey.GetPublicKey(privateKey.Key, withZeroByte: false);

            if (publicKey == null)
                throw new Exception("Failed to generate public key");

            return (privateKey.Key, publicKey);
        }

        public async Task<bool> UpdateRegistry(byte[] privateKey, byte[] publicKey, string dataKey, string data)
        {
            //Get current document
            var current = await GetRegistry(publicKey, dataKey);
            if (current == null)
                current = new RegistryEntry
                {
                    Key = dataKey
                };
            else
                current.Revision += 1;

            current.SetData(data);

            //Set new version
            return await SetRegistry(privateKey, publicKey, current);
        }

        public Task<bool> SkyDbSet(byte[] privateKey, byte[] publicKey, string dataKey, string data)
        {
            return SkyDbSet(privateKey, publicKey, dataKey, Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// SkyDB saves all data in external skyfiles
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> SkyDbSet(byte[] privateKey, byte[] publicKey, string dataKey, byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            {
                //Save data to Skynet file
                var response = await this.UploadFileAsync(dataKey, stream);

                var link = response.Skylink;

                //Save link to file in registry
                return await UpdateRegistry(privateKey, publicKey, dataKey, link);
            }
        }

        public async Task<string?> SkyDbGetAsString(byte[] publicKey, string dataKey)
        {
            var result = await this.SkyDbGet(publicKey, dataKey);
            if (result == null)
                return null;

            return Encoding.UTF8.GetString(result.Value.file);
        }

        public async Task<(byte[] file, string? contentType, SkynetFileMetadata metadata)?> SkyDbGet(byte[] publicKey, string dataKey)
        {
            var regEntry = await GetRegistry(publicKey, dataKey);

            if (regEntry == null)
                return null;

            //Get the data from the linked file
            return await this.DownloadFileAsByteArrayAsync(regEntry.GetDataAsString());
        }


        public async Task<bool> SetRegistry(byte[] privateKey, byte[] publicKey, RegistryEntry entry)
        {
            string hexPublicKey = BitConverter.ToString(publicKey).Replace("-", "");

            var signature = await entry.Sign(privateKey, publicKey);

            SetRegistryRequest req = new SetRegistryRequest();
            req.publickey.key = publicKey.Select(x => (int)x).ToArray();
            req.datakey = entry.GetHexKey();
            req.data = entry.Data.Select(x => (int)x).ToArray();
            req.revision = entry.Revision;
            req.signature = signature.Select(x => (int)x).ToArray();
            var result = await _api.SetRegistry(req);

            return result.IsSuccessStatusCode;

        }

        public async Task<RegistryEntry?> GetRegistry(byte[] publicKey, string dataKey, CancellationToken? cancellationToken = null)
        {
            var entry = new RegistryEntry() { Key = dataKey };

            string hexPublicKey = BitConverter.ToString(publicKey).Replace("-", "");

            if(cancellationToken == null)
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                cancellationToken = cts.Token;
            }

            //Get it back
            //var r = await _api.GetRegistry(hexPublicKey, hexDataKey);
            try
            {
                var response = await _api.GetRegistry("ed25519:" + hexPublicKey, entry.GetHexKey(), cancellationToken.Value);

                entry.Revision = response.Revision;
                entry.Data = Utils.HexStringToByteArray(response.Data);

                //TODO:     System.ArgumentException: Signature length is wrong. Got 128 instead of 64.
                //var validationResult = await Signer.ValidateAsync(Encoding.UTF8.GetBytes(response.Signature), entry.GetFullHash(), publicKey);
                //if (!validationResult)
                //    return null;

                return entry;
            }
            catch(TaskCanceledException)
            {
                //Task cancelled due to timeout, object is not there
                return null;
            }
        }


        private static void ReadFileHeaders(HttpResponseMessage httpResult, out string? contentType, out SkynetFileMetadata metadata)
        {
            string headerKey = "Skynet-File-Metadata";

            contentType = httpResult.Content.Headers.ContentType?.MediaType;

            if (httpResult.Headers.Contains(headerKey))
            {
                string? metadataJson = httpResult.Headers.GetValues(headerKey).FirstOrDefault();
                metadata = JsonConvert.DeserializeObject<SkynetFileMetadata>(metadataJson);
            }
            else
            {
                metadata = new SkynetFileMetadata();
            }
        }
    }
}
