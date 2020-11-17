using RestEase;
using SiaSkynet.Requests;
using SiaSkynet.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiaSkynet
{
    /// <summary>
    /// Skynet API docs: https://sia.tech/docs/#skynet
    /// </summary>
    public interface ISiaSkynetApi
    {
        [Post("skynet/skyfile")]
        Task<SkyfileResponse> UploadFile([Header("Content-Type")] MediaTypeHeaderValue contentType, [Query]string filename, [Body] Stream file);

        [Get("{skylink}")]
        Task<Stream> GetFileAsStream([Path]string skylink);

        [Get("{skylink}")]
        Task<HttpResponseMessage> GetFileAsHttpResponseMessage([Path]string skylink);

        [Head("{skylink}")]
        Task<HttpResponseMessage> GetFileHeadersAsHttpResponseMessage([Path]string skylink);

        [Post("skynet/registry")]
        Task<HttpResponseMessage> SetRegistry([Body] SetRegistryRequest req);

        [Get("skynet/registry")]
        Task<GetRegistryResponse> GetRegistry([Query]string publickey, [Query]string datakey);
    }
}
