using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PubnubApi.CBOR
{
    public class ItemHeader
    {
        public List<ItemTag> tags = new List<ItemTag>();
        public MajorType majorType { get; set; }
        public ulong additionalInfo { get; set; }
        public ulong value { get; set; }
        public bool indefinite { get; set; }
        public bool breakMarker { get; set; }

        internal ItemHeader()
        {

        }
        // Below are used only for the Encoder and should not ever be used by a 3rd party
        internal ItemHeader(MajorType type, ulong value, List<ItemTag> tags = null)
        {
            this.majorType = type;
            this.value = value;
            this.tags = tags;
        }

        internal static byte[] GetIndefiniteHeader(MajorType type)
        {
            return new byte[] { (byte)(((byte)type) << 5 | 31) };
        }

        internal byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();

            if (value < 24)
            {
                ms.WriteByte((byte)(((byte)majorType) << 5 | (byte)value));
            }
            else
            {
                if (value <= byte.MaxValue)
                {
                    ms.WriteByte((byte)(((byte)majorType) << 5 | 24));
                    ms.WriteByte((byte)value);
                }
                else if (value <= ushort.MaxValue)
                {
                    ms.WriteByte((byte)(((byte)majorType) << 5 | 25));

                    byte[] valueBytes = BitConverter.GetBytes((ushort)value);
                    Array.Reverse(valueBytes);

                    ms.Write(valueBytes, 0, valueBytes.Length);
                }
                else if (value <= uint.MaxValue)
                {
                    ms.WriteByte((byte)(((byte)majorType) << 5 | 26));

                    byte[] valueBytes = BitConverter.GetBytes((uint)value);
                    Array.Reverse(valueBytes);

                    ms.Write(valueBytes, 0, valueBytes.Length);
                }
                else if (value <= ulong.MaxValue)
                {
                    ms.WriteByte((byte)(((byte)majorType) << 5 | 27));

                    byte[] valueBytes = BitConverter.GetBytes((ulong)value);
                    Array.Reverse(valueBytes);

                    ms.Write(valueBytes, 0, valueBytes.Length);
                }
            }

            return ms.ToArray();
        }
    }
}
