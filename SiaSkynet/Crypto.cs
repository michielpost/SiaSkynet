using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Blake2b;
using Isopoh.Cryptography.SecureArray;

namespace SiaSkynet
{
    public static class Crypto
    {
        public static byte[] HashEncodeString(string data)
        {
            var encoded = Utils.EncodeString(data);
            return HashAll(encoded);
        }

        public static byte[] HashAll(params byte[][] data)
        {
            var hasher = Blake2B.Create(new Blake2BConfig() { OutputSizeInBytes = 32 }, SecureArray.DefaultCall);
            foreach(var d in data)
                hasher.Update(d);

            var hashAll = hasher.Finish();

            return hashAll;
        }
    }
}
