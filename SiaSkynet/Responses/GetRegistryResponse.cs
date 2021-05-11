using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SiaSkynet.Responses
{
    public class GetRegistryResponse
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("revision")]
        public int Revision { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }
}
