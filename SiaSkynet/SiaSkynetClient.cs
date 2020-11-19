﻿using MimeTypes;
using Newtonsoft.Json;
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
using System.Threading;
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
        public async Task<(string file, string? contentType, SkynetFileMetadata metadata)> DownloadFileAsStringAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                string file = await httpResult.Content.ReadAsStringAsync();

                ReadFileHeaders(httpResult, out string? contentType, out SkynetFileMetadata metadata);

                return (file, contentType, metadata);
            }
        }

        /// <summary>
        /// Download file as byte array, with contentType and metadata
        /// </summary>
        /// <param name="skylink"></param>
        /// <returns></returns>
        public async Task<(byte[] file, string? contentType, SkynetFileMetadata metadata)> DownloadFileAsByteArrayAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                byte[] file = await httpResult.Content.ReadAsByteArrayAsync();

                ReadFileHeaders(httpResult, out string? contentType, out SkynetFileMetadata metadata);

                return (file, contentType, metadata);
            }
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
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<bool> SkyDbSet(byte[] privateKey, byte[] publicKey, string dataKey, byte[] data, TimeSpan? timeout = null)
        {
            using (Stream stream = new MemoryStream(data))
            {
                //Save data to Skynet file
                var response = await this.UploadFileAsync(dataKey, stream);

                var link = response.Skylink;

                //Save link to file in registry
                return await UpdateRegistry(privateKey, publicKey, dataKey, link, timeout);
            }
        }

        /// <summary>
        /// Get SkyDB data as string
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<string?> SkyDbGetAsString(byte[] publicKey, string dataKey, TimeSpan? timeout = null)
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
        public async Task<(byte[] file, string? contentType, SkynetFileMetadata metadata)?> SkyDbGet(byte[] publicKey, string dataKey, TimeSpan? timeout = null)
        {
            var regEntry = await GetRegistry(publicKey, dataKey, timeout);

            if (regEntry == null)
                return null;

            //Get the data from the linked file
            return await this.DownloadFileAsByteArrayAsync(regEntry.GetDataAsString());
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

        /// <summary>
        /// Get data from the skynet registry
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="dataKey"></param>
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<RegistryEntry?> GetRegistry(byte[] publicKey, string dataKey, TimeSpan? timeout = null)
        {
            TimeSpan maxWait = timeout ?? TimeSpan.FromSeconds(2);

            var entry = new RegistryEntry() { Key = dataKey };
            string hexPublicKey = BitConverter.ToString(publicKey).Replace("-", "");

            var getRegistryTask = _api.GetRegistry("ed25519:" + hexPublicKey, entry.GetHexKey());


            var completedTask = await Task.WhenAny(getRegistryTask, Task.Delay(maxWait));
            if (completedTask == getRegistryTask)
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
        /// <param name="timeout">Time you want to wait for getting a SkyDB entry</param>
        /// <returns></returns>
        public async Task<bool> UpdateRegistry(byte[] privateKey, byte[] publicKey, string dataKey, string data, TimeSpan? timeout = null)
        {
            //Get current document
            var current = await GetRegistry(publicKey, dataKey, timeout);
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

        /// <summary>
        /// Read file headers from skynet file
        /// </summary>
        /// <param name="httpResult"></param>
        /// <param name="contentType"></param>
        /// <param name="metadata"></param>
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
