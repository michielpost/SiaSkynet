using System;
using System.Collections.Generic;
using System.Text;

namespace SiaSkynet.Responses
{
    public partial class SkyfileResponse
    {
        public string Skylink { get; set; }
        public string Merkleroot { get; set; }
        public long? Bitfield { get; set; }
    }
}
