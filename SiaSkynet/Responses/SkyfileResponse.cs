#if NET5_0
using System.Text.Json.Serialization;
#endif

namespace SiaSkynet.Responses
{
    public partial class SkyfileResponse
    {
#if NET5_0
        [JsonPropertyName("skylink")]
#endif
        public string Skylink { get; set; }

#if NET5_0
        [JsonPropertyName("merkleroot")]
#endif
        public string Merkleroot { get; set; }

#if NET5_0
        [JsonPropertyName("bitfield")]
#endif
        public long? Bitfield { get; set; }
    }
}
