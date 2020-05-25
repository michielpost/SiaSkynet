using RestEase;
using SiaSkynet.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
    }
}
