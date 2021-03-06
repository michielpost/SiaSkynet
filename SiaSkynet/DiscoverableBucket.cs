﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaSkynet
{
    /// <summary>
    /// https://github.com/SkynetHQ/skynet-js/blob/master/src/mysky/tweak.ts
    /// </summary>
    public class DiscoverableBucket
    {
        private readonly int Version = 1;
        private readonly byte[][] pathHashes;

#if NET5_0
        public string Path { get; init; }
#else
        public string Path { get; set; }
#endif


        public DiscoverableBucket(string path)
        {
            this.Path = path;
            var paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            this.pathHashes = paths.Select(x => Crypto.HashAll(Encoding.UTF8.GetBytes(x))).ToArray();
        }

        public byte[] GetHashedKey()
        {
            var encoding = Encode();
            return Crypto.HashAll(encoding);
        }

        private byte[] Encode()
        {
            int size = 1 + 32 * this.pathHashes.Length;
            byte[] buf = new byte[size];

            byte[] encodedVersion = BitConverter.GetBytes(Version);

            buf[0] = encodedVersion.First();
            var offset = 1;
            foreach (var pathLevel in this.pathHashes) {
                pathLevel.CopyTo(buf, offset);
                offset += 32;
            }

            return buf;
        }
    }
}
