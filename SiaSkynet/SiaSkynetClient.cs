using MimeTypes;
using RestEase;
using SiaSkynet.Responses;
using System;
using System.IO;
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

        public async Task<string> DownloadFileAsStringAsync(string skylink)
        {
            using (var httpResult = await _api.GetFileAsHttpResponseMessage(skylink))
            {
                string result = await httpResult.Content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
