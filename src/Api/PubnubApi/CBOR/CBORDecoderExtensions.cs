using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PubnubApi.CBOR
{
    public static class CBORDecoderExtensions
    {
        public static object DecodeCBORItem(this byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            CBORDecoder decode = new CBORDecoder(ms);
            return decode.ReadItem();
        }

        public static object DecodeCBORItem(this MemoryStream ms)
        {
            CBORDecoder decode = new CBORDecoder(ms);
            return decode.ReadItem();
        }

        public static object DecodeAllCBORItems(this byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            CBORDecoder decode = new CBORDecoder(ms);
            List<object> allItems = decode.ReadAllItems();
            return allItems;
        }

        public static object DecodeAllCBORItems(this MemoryStream ms)
        {
            CBORDecoder decode = new CBORDecoder(ms);
            return decode.ReadAllItems();
        }
    }
}
