using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class CryptorHeader
    {
        private const int IDENTIFIER_LENGTH = 4;
        //private static readonly byte[] NULL_IDENTIFIER = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] SENTINEL = new byte[] { 80, 78, 69, 68 }; // "PNED";
        private const byte MAX_VERSION = 1;

        public byte[] Identifier { get; set; }
        public int DataSize { get; set; }

        public CryptorHeader(byte[] identifier, int dataSize)
        {
            Identifier = identifier;
            DataSize = dataSize;
        }

        public int Length
        {
            get
            {
                if (DataSize < 255)
                {
                    return SENTINEL.Length + 1 + IDENTIFIER_LENGTH + 1 + DataSize;
                }

                return SENTINEL.Length + 1 + IDENTIFIER_LENGTH + 3 + DataSize;
            }
        }

        public byte Version
        {
            get { return 1; }
        }

        public static CryptorHeader FromBytes(byte[] data)
        {
            if (data.Length < 4 || !data.Take(4).SequenceEqual(SENTINEL))
            {
                return null; // Malformed or no header
            }

            byte version = data[4];

            if (version == 0)
            {
                throw new PNException("decryption error");
            }
            if (version > MAX_VERSION)
            {
                throw new PNException("unknown cryptor error");
            }

            byte[] identifier = data.Skip(5).Take(IDENTIFIER_LENGTH).ToArray();
            if (Encoding.UTF8.GetString(identifier,0, identifier.Length).Trim('\0').Length < 4)
            {
                throw new PNException("decryption error");
            }

            int dataSizeOffset = 5 + IDENTIFIER_LENGTH;

            int dataSize;
            if (dataSizeOffset < data.Length)
            {
                byte dataSizeByte = data[dataSizeOffset];
                dataSize = dataSizeByte < 255 ? dataSizeByte : BitConverter.ToUInt16(data, dataSizeOffset + 1);
            }
            else
            {
                dataSize = 0;
            }

            return new CryptorHeader(identifier, dataSize);
        }

        public static CryptorHeader FromFile(string sourceFile)
        {
            #if !NETSTANDARD10 && !NETSTANDARD11
            using (System.IO.FileStream fs = new System.IO.FileStream(sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                byte[] headerBytes = new byte[5 + IDENTIFIER_LENGTH + 3];
                fs.Read(headerBytes, 0, headerBytes.Length);
                return FromBytes(headerBytes);
            }
            #else
            throw new NotSupportedException("FileStream not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
            #endif
        }
        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(SENTINEL);
            bytes.Add(Version);
            bytes.AddRange(Identifier);

            if (DataSize < 255)
            {
                bytes.Add((byte)DataSize);
            }
            else
            {
                bytes.Add(255);
                bytes.AddRange(BitConverter.GetBytes((ushort)DataSize));
            }

            return bytes.ToArray();
        }
    }
}
