#if NET5_0
using System.Text.Json.Serialization;
#endif

namespace SiaSkynet.Responses
{
    public class SkynetFileMetadata
    {
#if NET5_0
       [JsonPropertyName("filename")]
#endif
        public string? Filename { get; set; }
    }
}
