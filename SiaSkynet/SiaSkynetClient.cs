using RestEase;
using System;

namespace SiaSkynet
{
    public class SiaSkynetClient
    {
        private const string apiBaseUrl = "https://siasky.net/";

        public static ISiaSkynetApi GetClient(string baseUrl = apiBaseUrl)
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
    }
}
