using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet
{
    public class RegistryKey
    {
        public byte[] DataKeyBytes { get; set; }
        public string Key { get; set; }

        public RegistryKey(string key)
        {
            this.Key = key;
            var encodedKey = Utils.EncodeString(key);
            DataKeyBytes = Crypto.HashAll(encodedKey);
        }

        public RegistryKey(DiscoverableBucket bucket)
        {
            this.DataKeyBytes = bucket.GetHashedKey();
            this.Key = bucket.Path;
        }

        public string GetHexKey()
        {
            return BitConverter.ToString(DataKeyBytes).Replace("-", "");
        }
    }
}
