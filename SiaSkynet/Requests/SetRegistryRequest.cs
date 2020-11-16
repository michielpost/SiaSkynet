using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet.Requests
{
    public class SetRegistryRequest
    {
        public Publickey publickey { get; set; } = new Publickey();
        public string datakey { get; set; }
        public int revision { get; set; } = 0;
        public int[] data { get; set; }
        public int[] signature { get; set; }
    }

    public class Publickey
    {
        public string algorithm { get; set; } = "ed25519";
        public int[] key { get; set; }
    }
}
