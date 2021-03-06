﻿using SiaSkynet.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet
{
    public static class Utils
    {
        /// <summary>
        /// Encode string for Skynet Registry
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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

        public static byte[] EncodeData(byte[] data)
        {
            byte[] encodedDataLength = BitConverter.GetBytes(data.Length);
            Array.Resize(ref encodedDataLength, 8);
            byte[] encodedData = data;
            byte[] totalEncodedData = encodedDataLength.Concat(encodedData).ToArray();

            var totalData = 8 + data.Length;
            if (totalData != totalEncodedData.Length)
            {
                throw new Exception("Invalid length");
            }

            return totalEncodedData;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] Sign(this RegistryEntry entry, byte[] privateKey, byte[] publicKey)
        {
            var hashAll = entry.GetFullHash();

            var signature = Chaos.NaCl.Ed25519.Sign(hashAll, privateKey);
            //var validationResult = await Signer.ValidateAsync(signature, hashAll, publicKey);

            return signature;
        }

        public static byte[] Encrypt(byte[] data, byte[] privateKey)
        {
            byte[] nonce = privateKey[0..24];
            byte[] shortKey = privateKey[0..32];

           return Chaos.NaCl.XSalsa20Poly1305.Encrypt(data, shortKey, nonce);
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] privateKey)
        {
            byte[] nonce = privateKey[0..24];
            byte[] shortKey = privateKey[0..32];

            return Chaos.NaCl.XSalsa20Poly1305.TryDecrypt(ciphertext, shortKey, nonce);
        }
    }
}
