using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto
{
    public class CryptorHeader
    {
        private const int IDENTIFIER_LENGTH = 4;
        //private static readonly byte[] NULL_IDENTIFIER = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] SENTINEL = new byte[] { 0x50, 0x4E, 0x45, 0x44 }; // "PNED"
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
                else
                {
                    return SENTINEL.Length + 1 + IDENTIFIER_LENGTH + 3 + DataSize;
                }
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

            if (version == 0 || version > MAX_VERSION)
            {
                return null; // Unknown version
            }

            byte[] identifier = data.Skip(5).Take(IDENTIFIER_LENGTH).ToArray();

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
