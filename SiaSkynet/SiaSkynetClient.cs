using MimeTypes;
using RestEase;
using SiaSkynet.Requests;
using SiaSkynet.Responses;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet
{
    /// <summary>
    /// Client to interact with Skynet
    /// </summary>
    public class SiaSkynetClient
    {
        private const string apiBaseUrl = "https://siasky.net/";

        private ISiaSkynetApi _api;

        public SiaSkynetClient(string baseUrl = apiBaseUrl, string? apiKey = null, HttpClient? client = null)
        {
            if (client == null)
                client = new HttpClient();

            _api = GetApi(client, baseUrl, apiKey);

        }
        public ISiaSkynetApi GetApi(HttpClient client, string baseUrl = apiBaseUrl, string? apiKey = null)
        {
            client.BaseAddress = new Uri(baseUrl);

            var nbApi = new RestClient(client).For<ISiaSkynetApi>();
            nbApi.ApiKey = apiKey;

            return nbApi;
        }

        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task<SkyfileResponse> UploadFileAsync(string fileName, Stream file)
        {
            string extensions = Path.GetExtension(fileName);
            var contentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(extensions));
            return _api.UploadFile(contentType, fileName, file);
        }

        /// <summary>
        /// Download a file as a stream
        /// </summary>
        /// <param name="skylink"></param>
        /// <returns></returns>
        public Task<Stream> DownloadFileAsStreamAsync(string skylink)
        {
            return _api.GetFileAsStream(skylink);
        }

        /// <summary>
        /// Download file as string, with contentType and metadata
        /// </summary>
        /// <param name="skylink"></param>
        /// <returns></returns>
        public async Task<(string file, string? contentType, string? fileName)> DownloadFileAsStringAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                string file = await httpResult.Content.ReadAsStringAsync();

                ReadFileHeaders(httpResult, out string? contentType, out string? fileName);

                return (file, contentType, fileName);
            }
        }

        /// <summary>
        /// Download file as byte array, with contentType and metadata
        /// </summary>
        /// <param name="skylink"></param>
        /// <returns></returns>
        public async Task<(byte[] file, string? contentType, string? fileName)> DownloadFileAsByteArrayAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                byte[] file = await httpResult.Content.ReadAsByteArrayAsync();

                ReadFileHeaders(httpResult, out string? contentType, out string? fileName);

                return (file, contentType, fileName);
            }
        }

        public Task<SkynetFileMetadata> GetMetadata(string skylink)
        {
            return _api.GetMetadata(skylink);
        }

        /// <summary>
        /// Generates a predicatable key pair based on the seed
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static (byte[] privateKey, byte[] publicKey) GenerateKeys(string seed)
        {
            var masterKey = GetMasterKeyFromSeed(seed);
            Chaos.NaCl.Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] privateKey, masterKey);

            if (publicKey == null)
                throw new Exception("Failed to generate public key");

            return (privateKey, publicKey);
        }

        /// <summary>
        /// Generate a private key
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        private static byte[] GetMasterKeyFromSeed(string seed)
        {
            SHA256Managed hasher = new SHA256Managed();
            byte[] keyBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(seed));
            var i = keyBytes.AsSpan();
            return i.Slice(0, 32).ToArray();
        }

        /// <summary>
        /// Save string data to SkyDB
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> SkyDbSetAsString(byte[] privateKey, byte[] publicKey, RegistryKey dataKey, string data)
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
        /// <param name="revision">optional forces revision</param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<bool> SkyDbSet(byte[] privateKey, byte[] publicKey, RegistryKey dataKey, byte[] data, int? revision = null, TimeSpan? timeout = null)
        {
            using (Stream stream = new MemoryStream(data))
            {
                //Save data to Skynet file
                var response = await this.UploadFileAsync(dataKey.Key, stream);

                var link = response.Skylink;

                //Save link to file in registry
                return await UpdateRegistry(privateKey, publicKey, dataKey, link, revision, timeout);
            }
        }

        /// <summary>
        /// Get SkyDB data as string
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<string?> SkyDbGetAsString(byte[] publicKey, RegistryKey dataKey, TimeSpan? timeout = null)
        {
            var result = await this.SkyDbGet(publicKey, dataKey, timeout);
            if (result == null)
                return null;

            return Encoding.UTF8.GetString(result.Value.file);
        }

        /// <summary>
        /// Get SkyDB data based on a key
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<(byte[] file, string? contentType, string? fileName, RegistryEntry? registryEntry)?> SkyDbGet(byte[] publicKey, RegistryKey dataKey, TimeSpan? timeout = null)
        {
            var regEntry = await GetRegistry(publicKey, dataKey, timeout);

            if (regEntry == null)
                return null;

            //Get the data from the linked file
            var fileResult = await this.DownloadFileAsByteArrayAsync(regEntry.GetDataAsString());

            return (fileResult.file, fileResult.contentType, fileResult.fileName, regEntry);
        }

        /// <summary>
        /// Save data to the Skynet Registry
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task<bool> SetRegistry(byte[] privateKey, byte[] publicKey, RegistryEntry entry)
        {
            string hexPublicKey = BitConverter.ToString(publicKey).Replace("-", "");

            var signature = entry.Sign(privateKey, publicKey);

            SetRegistryRequest req = new SetRegistryRequest();
            req.publickey.key = publicKey.Select(x => (int)x).ToArray();
            req.datakey = entry.Key.GetHexKey();
            req.data = entry.Data.Select(x => (int)x).ToArray();
            req.revision = entry.Revision;
            req.signature = signature.Select(x => (int)x).ToArray();
            var result = await _api.SetRegistry(req);

            return result.IsSuccessStatusCode;

        }

        /// <summary>
        /// Get data from the skynet registry
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<RegistryEntry?> GetRegistry(byte[] publicKey, RegistryKey dataKey, TimeSpan? timeout = null)
        {
            TimeSpan maxWait = timeout ?? TimeSpan.FromSeconds(2);

            var entry = new RegistryEntry(dataKey);
            string hexPublicKey = BitConverter.ToString(publicKey).Replace("-", "");

            var getRegistryTask = _api.GetRegistry("ed25519:" + hexPublicKey, entry.Key.GetHexKey());


            var completedTask = await Task.WhenAny(getRegistryTask, Task.Delay(maxWait));
            if (completedTask == getRegistryTask)
            {
                try
                {
                    var response = getRegistryTask.Result;
                    entry.Revision = response.Revision;
                    entry.Data = Utils.HexStringToByteArray(response.Data);

                    //TODO:     System.ArgumentException: Signature length is wrong. Got 128 instead of 64.
                    //var validationResult = await Signer.ValidateAsync(Encoding.UTF8.GetBytes(response.Signature), entry.GetFullHash(), publicKey);
                    //if (!validationResult)
                    //    return null;

                    return entry;
                }
                catch(Exception ex) //Skynet portal returns a 404 when registry key is not found
                {
                    return null;
                }
            }

            //Task cancelled due to timeout, object is not there
            return null;
        }

        /// <summary>
        /// Update skynet registry entry
        /// Gets the current version, updates the revision and stores the new data
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="data"></param>
        /// <param name="revision">optional forced revision</param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<bool> UpdateRegistry(byte[] privateKey, byte[] publicKey, RegistryKey dataKey, string data, int? revision = null, TimeSpan? timeout = null)
        {
            //Get current document
            RegistryEntry? current = new RegistryEntry(dataKey);

            if (!revision.HasValue)
            {
                current = await GetRegistry(publicKey, dataKey, timeout);
                if (current == null)
                    current = new RegistryEntry(dataKey);
                else
                    current.Revision += 1;
            }
            else
            {
                //Do not get entry to save time, but set forced revision
                current.Revision = revision.Value;
            }

            current.SetData(data);

            //Set new version
            return await SetRegistry(privateKey, publicKey, current);
        }

        /// <summary>
        /// Read file headers from skynet file
        /// </summary>
        /// <param name="httpResult"></param>
        /// <param name="contentType"></param>
        /// <param name="metadata"></param>
        private static void ReadFileHeaders(HttpResponseMessage httpResult, out string? contentType, out string? fileName)
        {
            contentType = httpResult.Content.Headers.ContentType?.MediaType;
            fileName = httpResult.Content.Headers.ContentDisposition?.FileName?.Replace("\"", string.Empty);
        }
    }
}
