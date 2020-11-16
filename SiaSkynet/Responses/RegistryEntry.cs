using Isopoh.Cryptography.Blake2b;
using Isopoh.Cryptography.SecureArray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet.Responses
{
    public class RegistryEntry
    {
        public string Key { get; set; }
        public int Revision { get; set; } = 0;
        public byte[] Data { get; set; } = new byte[0];

        public void SetData(string data)
        {
            Data = Encoding.UTF8.GetBytes(data);
        }

        public string GetDataAsString()
        {
            return Encoding.UTF8.GetString(Data);
        }

        public byte[] GetEncodedRevision()
        {
            byte[] encodedRevision = BitConverter.GetBytes(Revision);
            Array.Resize(ref encodedRevision, 8);

            return encodedRevision;
        }

        public byte[] GetEncodedData()
        {
            string dataString = Encoding.UTF8.GetString(Data);
            return Utils.EncodeString(dataString);
        }

        internal byte[] GetEncodedKey()
        {
            return Utils.EncodeString(Key);
        }

        public byte[] GetHashedKey()
        {
            return Blake2B.ComputeHash(GetEncodedKey(), new Blake2BConfig() { OutputSizeInBytes = 32 }, SecureArray.DefaultCall);
        }

        public string GetHexKey()
        {
            return BitConverter.ToString(GetHashedKey()).Replace("-", "");
        }

        public byte[] GetFullHash()
        {
            var hasher = Blake2B.Create(new Blake2BConfig() { OutputSizeInBytes = 32 }, SecureArray.DefaultCall);
            hasher.Update(GetHashedKey());
            hasher.Update(GetEncodedData());
            hasher.Update(GetEncodedRevision());
            var hashAll = hasher.Finish();

            return hashAll;
        }
    }
}
