using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet.Responses
{
    public class GetRegistryResponse
    {
        public string Data { get; set; }
        public int Revision { get; set; }
        public string Signature { get; set; }
    }
}
