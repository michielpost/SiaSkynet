using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet.Responses
{
    public class RegistryEntry
    {
        private readonly byte[] encodedKey;

        public RegistryEntry(string key)
        {
            encodedKey = Utils.EncodeString(key);
        }

        public RegistryEntry(byte[] key)
        {
            this.encodedKey = Utils.EncodeData(key);
        }

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

        public byte[] GetHashedKey()
        {
            return Crypto.HashAll(encodedKey);
        }

        public string GetHexKey()
        {
            return BitConverter.ToString(GetHashedKey()).Replace("-", "");
        }

        public byte[] GetFullHash()
        {
            return Crypto.HashAll(GetHashedKey(), GetEncodedData(), GetEncodedRevision());
        }
    }
}
