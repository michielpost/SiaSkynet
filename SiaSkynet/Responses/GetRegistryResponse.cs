#if NET5_0
using System.Text.Json.Serialization;
#endif

namespace SiaSkynet.Responses
{
    public class GetRegistryResponse
    {
#if NET5_0
        [JsonPropertyName("data")]
#endif
        public string Data { get; set; }

#if NET5_0
        [JsonPropertyName("revision")]
#endif
        public int Revision { get; set; }

#if NET5_0
        [JsonPropertyName("signature")]
#endif
        public string Signature { get; set; }
    }
}
