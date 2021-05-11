using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SiaSkynet.Responses
{
    public class SkynetFileMetadata
    {
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
    }
}
