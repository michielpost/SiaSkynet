using Ed25519;
using Isopoh.Cryptography.Blake2b;
using Isopoh.Cryptography.SecureArray;
using SiaSkynet.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet
{
    public static class Utils
    {
        public static byte[] EncodeString(string data)
        {
            byte[] encodedDataLength = BitConverter.GetBytes(data.Length);
            Array.Resize(ref encodedDataLength, 8);
            byte[] encodedData = Encoding.UTF8.GetBytes(data);
            byte[] totalEncodedData = encodedDataLength.Concat(encodedData).ToArray();

            var totalData = 8 + data.Length;
            if (totalData != totalEncodedData.Length)
            {
                throw new Exception("Invalid length");
            }

            return totalEncodedData;
        }

        //public static string ByteArrayToString(byte[] ba)
        //{
        //    StringBuilder hex = new StringBuilder(ba.Length * 2);
        //    foreach (byte b in ba)
        //        hex.AppendFormat("{0:x2}", b);
        //    return hex.ToString();
        //}

        public static byte[] HexStringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static async Task<byte[]> Sign(this RegistryEntry entry, byte[] privateKey, byte[] publicKey)
        {
            string hexPublicKey = BitConverter.ToString(publicKey).Replace("-", "");

            var hashAll = entry.GetFullHash();

            var signature = await Signer.SignAsync(hashAll, privateKey, publicKey);
            //var validationResult = await Signer.ValidateAsync(signature, hashAll, publicKey);

            return signature;
        }
    }
}
