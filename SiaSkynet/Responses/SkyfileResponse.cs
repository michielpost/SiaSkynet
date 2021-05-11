using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SiaSkynet.Responses
{
    public partial class SkyfileResponse
    {
        [JsonPropertyName("skylink")]
        public string Skylink { get; set; }

        [JsonPropertyName("merkleroot")]
        public string Merkleroot { get; set; }

        [JsonPropertyName("bitfield")]
        public long? Bitfield { get; set; }
    }
}
