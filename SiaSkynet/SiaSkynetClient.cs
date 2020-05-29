using MimeTypes;
using Newtonsoft.Json;
using RestEase;
using SiaSkynet.Responses;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task<(string file, string contentType, SkynetFileMetadata metadata)> DownloadFileAsStringAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                string file = await httpResult.Content.ReadAsStringAsync();

                ReadFileHeaders(httpResult, out string contentType, out SkynetFileMetadata metadata);

                return (file, contentType, metadata);
            }
        }

        public async Task<(byte[] file, string contentType, SkynetFileMetadata metadata)> DownloadFileAsByteArrayAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                byte[] file = await httpResult.Content.ReadAsByteArrayAsync();

                ReadFileHeaders(httpResult, out string contentType, out SkynetFileMetadata metadata);

                return (file, contentType, metadata);
            }
        }

        private static void ReadFileHeaders(HttpResponseMessage httpResult, out string contentType, out SkynetFileMetadata metadata)
        {
            string headerKey = "Skynet-File-Metadata";

            contentType = httpResult.Content.Headers.ContentType.MediaType;

            if (httpResult.Headers.Contains(headerKey))
            {
                string metadataJson = httpResult.Headers.GetValues(headerKey).FirstOrDefault();
                metadata = JsonConvert.DeserializeObject<SkynetFileMetadata>(metadataJson);
            }
            else
            {
                metadata = new SkynetFileMetadata();
            }
        }
    }
}
