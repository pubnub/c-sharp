using MockServer;
using System.Diagnostics;
using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;
//using CBOR;
//using CBOR.Tags;
using System.Numerics;

namespace PubNubMessaging.Tests
{
    public class UnitTestLog : IMockServerLog
    {
        private LoggingMethod.Level _logLevel = LoggingMethod.Level.Info;
        private string logFilePath = "";

        public UnitTestLog()
        {
            // Get folder path may vary based on environment
            string folder = System.IO.Directory.GetCurrentDirectory();
            //For console
            //string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // For iOS
            System.Diagnostics.Debug.WriteLine(folder);
            logFilePath = System.IO.Path.Combine(folder, "pubnubUnitTestLog.log");
            Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
        }

        /// <summary>
        /// Set the Log Level
        /// </summary>
        public LoggingMethod.Level LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        /// <summary>
        /// Write a log
        /// </summary>
        /// <param name="log">Log string</param>
        public void WriteToLog(string log)
        {
            Trace.WriteLine(log);
            Trace.Flush();
        }
    }

}

//namespace CBOR
//{
//    public static class CBORDecoderExtensions
//    {
//        public static object DecodeCBORItem(this byte[] data)
//        {
//            MemoryStream ms = new MemoryStream(data);
//            CBORDecoder decode = new CBORDecoder(ms);
//            return decode.ReadItem();
//        }

//        public static object DecodeCBORItem(this MemoryStream ms)
//        {
//            CBORDecoder decode = new CBORDecoder(ms);
//            return decode.ReadItem();
//        }

//        public static object DecodeAllCBORItems(this byte[] data)
//        {
//            MemoryStream ms = new MemoryStream(data);
//            CBORDecoder decode = new CBORDecoder(ms);
//            List<object> allItems = decode.ReadAllItems();
//            return allItems;
//        }

//        public static object DecodeAllCBORItems(this MemoryStream ms)
//        {
//            CBORDecoder decode = new CBORDecoder(ms);
//            return decode.ReadAllItems();
//        }
//    }

//    public class CBORDecoder
//    {
//        Stream buffer;
//        public CBORDecoder(Stream s)
//        {
//            TagRegistry.RegisterTagTypes();
//            buffer = s;
//        }

//        public CBORDecoder(byte[] data)
//        {
//            TagRegistry.RegisterTagTypes();
//            buffer = new MemoryStream(data);
//        }

//        public void SetDataSource(byte[] data)
//        {
//            buffer = new MemoryStream(data);
//        }

//        public void SetDataSource(Stream s)
//        {
//            buffer = s;
//        }



//        public object ReadItem()
//        {
//            ItemHeader header = ReadHeader();
//            object dataItem = null;
//            switch (header.majorType)
//            {
//                case MajorType.UNSIGNED_INT:
//                    if (header.value == 0)
//                    {
//                        dataItem = header.additionalInfo;
//                    }
//                    else
//                    {

//                        dataItem = (ulong)header.value;
//                    }
//                    break;
//                case MajorType.NEGATIVE_INT:
//                    if (header.value == 0)
//                    {
//                        dataItem = ((long)(header.additionalInfo + 1) * -1);
//                    }
//                    else
//                    {

//                        dataItem = ((long)(header.value + 1) * -1);
//                    }
//                    break;
//                case MajorType.BYTE_STRING:
//                    ulong byteLength = header.value == 0 ? header.additionalInfo : header.value;

//                    byte[] bytes = new byte[byteLength];
//                    for (ulong x = 0; x < byteLength; x++)
//                    {
//                        bytes[x] = (byte)buffer.ReadByte();
//                    }

//                    dataItem = bytes;
//                    break;
//                case MajorType.TEXT_STRING:
//                    ulong stringLength = header.value == 0 ? header.additionalInfo : header.value;

//                    byte[] data = new byte[stringLength];
//                    for (ulong x = 0; x < stringLength; x++)
//                    {
//                        data[x] = (byte)buffer.ReadByte();
//                    }

//                    dataItem = Encoding.UTF8.GetString(data);
//                    break;
//                case MajorType.ARRAY:
//                    ArrayList array = new ArrayList();
//                    if (header.indefinite == false)
//                    {
//                        ulong elementCount = header.additionalInfo;
//                        if (header.value != 0) { elementCount = header.value; }

//                        for (ulong x = 0; x < elementCount; x++)
//                        {
//                            array.Add(ReadItem());
//                        }
//                    }
//                    else
//                    {
//                        while (PeekBreak() == false)
//                        {
//                            array.Add(ReadItem());
//                        }
//                        buffer.ReadByte();
//                    }

//                    dataItem = array;
//                    break;
//                case MajorType.MAP:
//                    Dictionary<string, object> dict = new Dictionary<string, object>();

//                    ulong pairCount = header.value == 0 ? header.additionalInfo : header.value;
//                    for (ulong x = 0; x < pairCount; x++)
//                    {
//                        string itemKey="";
//                        object itemKeyObj = ReadItem();
//                        if (itemKeyObj.GetType() == typeof(byte[]))
//                        {
//                            itemKey = Encoding.UTF8.GetString((byte[])itemKeyObj);
//                        }
//                        else
//                        {
//                            itemKey = itemKeyObj.ToString();
//                        }
//                        object itemVal = ReadItem();
//                        dict.Add(itemKey, itemVal);
//                    }

//                    dataItem = dict;
//                    break;
//                case MajorType.FLOATING_POINT_OR_SIMPLE:
//                    if (header.additionalInfo < 24)
//                    {
//                        switch (header.additionalInfo)
//                        {
//                            case 20:
//                                return false;
//                            case 21:
//                                return true;
//                            case 22:
//                                return null;
//                            case 23:
//                                return new UndefinedValue();
//                        }
//                    }

//                    if (header.additionalInfo == 24)
//                    {
//                        // no simple value in range 32-255 has been defined
//                        throw new Exception();
//                    }

//                    if (header.additionalInfo == 25)
//                    {
//                        //Half halfValue = Half.ToHalf(BitConverter.GetBytes(header.value), 0);

//                        //dataItem = (float)halfValue;
//                    }
//                    else if (header.additionalInfo == 26)
//                    {
//                        // single (32 bit) precision float value
//                        dataItem = BitConverter.ToSingle(BitConverter.GetBytes(header.value), 0);
//                    }
//                    else if (header.additionalInfo == 27)
//                    {
//                        // double (64 bit) precision float value
//                        dataItem = BitConverter.ToDouble(BitConverter.GetBytes(header.value), 0);
//                    }
//                    else
//                    {
//                        throw new Exception();
//                    }
//                    // unknown simple value type
//                    break;
//            }

//            for (int x = header.tags.Count - 1; x >= 0; x--)
//            {
//                if (header.tags[x].isDataSupported(dataItem))
//                {
//                    dataItem = header.tags[x].processData(dataItem);
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            return dataItem;
//        }

//        public List<ItemTag> ReadTags()
//        {
//            List<ItemTag> tags = new List<ItemTag>();

//            byte b = (byte)buffer.ReadByte();

//            while (b >> 5 == 6)
//            {


//                ulong extraInfo = (ulong)b & 0x1f;
//                ulong tagNum = 0;
//                if (extraInfo >= 24 && extraInfo <= 27)
//                {
//                    tagNum = readUnsigned(1 << (b - 24));

//                }
//                else
//                {
//                    tagNum = extraInfo;
//                }
//                ItemTag tag = TagRegistry.getTagInstance(tagNum);
//                tags.Add(tag);
//                b = (byte)buffer.ReadByte();
//            }
//            buffer.Seek(-1, SeekOrigin.Current);
//            return tags;
//        }

//        public List<Object> ReadAllItems()
//        {
//            List<Object> items = new List<object>();
//            while (buffer.Position < buffer.Length)
//            {
//                items.Add(ReadItem());
//            }

//            return items;
//        }
//        public ItemHeader ReadHeader()
//        {
//            ItemHeader header = new ItemHeader();

//            header.tags = ReadTags();

//            ulong size = 0;
//            byte b = (byte)buffer.ReadByte();

//            if (b == 0xFF)
//            {
//                header.breakMarker = true;
//                return header;
//            }

//            header.majorType = (MajorType)(b >> 5);

//            b &= 0x1f;
//            header.additionalInfo = (ulong)b;
//            if (b >= 24 && b <= 27)
//            {
//                b = (byte)(1 << (b - 24));
//                header.value = readUnsigned(b);
//            }
//            else if (b > 27 && b < 31)
//            {
//                throw new Exception();
//            }
//            else if (b == 31)
//            {
//                header.indefinite = true;
//            }
//            return header;
//        }

//        public MajorType PeekType()
//        {
//            long pos = buffer.Position;
//            MajorType type = ReadHeader().majorType;
//            buffer.Seek(pos, SeekOrigin.Begin);
//            return type;
//        }

//        public bool PeekBreak()
//        {
//            long pos = buffer.Position;
//            bool isBreak = ReadHeader().breakMarker;
//            buffer.Seek(pos, SeekOrigin.Begin);
//            return isBreak;
//        }

//        public ulong PeekSize()
//        {
//            long pos = buffer.Position;
//            ItemHeader header = ReadHeader();
//            ulong size = header.value != 0 ? header.value : header.additionalInfo;
//            buffer.Seek(pos, SeekOrigin.Begin);
//            return size;
//        }

//        public bool PeekIndefinite()
//        {
//            long pos = buffer.Position;
//            bool isIndefinite = ReadHeader().indefinite;
//            buffer.Seek(pos, SeekOrigin.Begin);
//            return isIndefinite;
//        }

//        private ulong readUnsigned(int size)
//        {
//            byte[] buff = new byte[8];

//            buffer.Read(buff, 0, size);

//            Array.Reverse(buff, 0, size);

//            return BitConverter.ToUInt64(buff, 0);

//        }
//    }

//    public abstract class ItemTag
//    {
//        public ulong tagNumber;

//        public abstract object processData(object data);


//        public abstract bool isDataSupported(object data);
//    }

//    public enum MajorType : byte
//    {
//        UNSIGNED_INT = 0,
//        NEGATIVE_INT = 1,
//        BYTE_STRING = 2,
//        TEXT_STRING = 3,
//        ARRAY = 4,
//        MAP = 5,
//        TAG = 6,
//        FLOATING_POINT_OR_SIMPLE = 7
//    }

//    public class ItemHeader
//    {
//        public List<ItemTag> tags = new List<ItemTag>();
//        public MajorType majorType { get; set; }
//        public ulong additionalInfo { get; set; }
//        public ulong value { get; set; }
//        public bool indefinite { get; set; }
//        public bool breakMarker { get; set; }

//        internal ItemHeader()
//        {

//        }
//        // Below are used only for the Encoder and should not ever be used by a 3rd party
//        internal ItemHeader(MajorType type, ulong value, List<ItemTag> tags = null)
//        {
//            this.majorType = type;
//            this.value = value;
//            this.tags = tags;
//        }

//        internal static byte[] GetIndefiniteHeader(MajorType type)
//        {
//            return new byte[] { (byte)(((byte)type) << 5 | 31) };
//        }

//        internal byte[] ToByteArray()
//        {
//            MemoryStream ms = new MemoryStream();

//            if (value < 24)
//            {
//                ms.WriteByte((byte)(((byte)majorType) << 5 | (byte)value));
//            }
//            else
//            {
//                if (value <= byte.MaxValue)
//                {
//                    ms.WriteByte((byte)(((byte)majorType) << 5 | 24));
//                    ms.WriteByte((byte)value);
//                }
//                else if (value <= ushort.MaxValue)
//                {
//                    ms.WriteByte((byte)(((byte)majorType) << 5 | 25));

//                    byte[] valueBytes = BitConverter.GetBytes((ushort)value);
//                    Array.Reverse(valueBytes);

//                    ms.Write(valueBytes, 0, valueBytes.Length);
//                }
//                else if (value <= uint.MaxValue)
//                {
//                    ms.WriteByte((byte)(((byte)majorType) << 5 | 26));

//                    byte[] valueBytes = BitConverter.GetBytes((uint)value);
//                    Array.Reverse(valueBytes);

//                    ms.Write(valueBytes, 0, valueBytes.Length);
//                }
//                else if (value <= ulong.MaxValue)
//                {
//                    ms.WriteByte((byte)(((byte)majorType) << 5 | 27));

//                    byte[] valueBytes = BitConverter.GetBytes((ulong)value);
//                    Array.Reverse(valueBytes);

//                    ms.Write(valueBytes, 0, valueBytes.Length);
//                }
//            }

//            return ms.ToArray();
//        }
//    }

//    class UndefinedValue
//    {
//    }
//}
//namespace CBOR.Tags
//{
//    class Base64Tag : ItemTag
//    {
//        public static ulong[] TAG_NUM = new ulong[] { 33, 34 };

//        public Base64Tag(ulong tagNum)
//        {
//            this.tagNumber = tagNum;
//        }

//        public override object processData(object data)
//        {
//            if (this.tagNumber == 33)
//            {
//                String s = (data as string);
//                s = s.Replace("_", "/");
//                s = s.Replace("-", "+");
//                s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');

//                byte[] decoded = System.Convert.FromBase64String(s);

//                String decodedString = System.Text.Encoding.UTF8.GetString(decoded);

//                return new Uri(decodedString);
//            }
//            else
//            {
//                byte[] decoded = System.Convert.FromBase64String((data as string));
//                return decoded;
//            }

//        }

//        public override bool isDataSupported(object data)
//        {
//            return (data is string);
//        }
//    }

//    public class TagRegistry
//    {
//        public static Dictionary<ulong, Type> tagMap = new Dictionary<ulong, Type>();
//        public static bool isInit = false;

//        public static void RegisterTagTypes()
//        {
//            if (!isInit)
//            {
//                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
//                {
//                    foreach (var type in asm.GetTypes())
//                    {
//                        if (type.BaseType == typeof(ItemTag))
//                        {
//                            try
//                            {
//                                ulong[] tagNum = (ulong[])type.GetField("TAG_NUM").GetValue(null);

//                                foreach (ulong l in tagNum)
//                                {
//                                    tagMap.Add(l, type);
//                                }
//                            }
//                            catch (Exception)
//                            {
//                            }

//                        }
//                    }
//                }
//            }
//            isInit = true;
//        }

//        public static ItemTag getTagInstance(ulong tagId)
//        {
//            if (tagMap.ContainsKey(tagId))
//            {
//                return (ItemTag)Activator.CreateInstance(tagMap[tagId], tagId);
//            }
//            else
//            {
//                return new UnknownTag(tagId);
//            }

//        }

//        internal static void registerTag(ulong p, Type type)
//        {
//            if (tagMap.ContainsKey(p) == false)
//            {
//                tagMap.Add(p, type);
//            }
//        }
//    }

//    public class BigIntegerTag : ItemTag
//    {
//        public static ulong[] TAG_NUM = new ulong[] { 2, 3 };
//        public BigIntegerTag(ulong tagId)
//        {
//            this.tagNumber = tagId;
//        }

//        public override object processData(object data)
//        {
//            Array.Reverse((Array)data);
//            BigInteger bi = new BigInteger((byte[])data);

//            if (this.tagNumber == 2)
//            {
//                return bi;
//            }
//            else
//            {
//                return BigInteger.Subtract(BigInteger.MinusOne, bi);
//            }
//        }

//        public override bool isDataSupported(object data)
//        {
//            try
//            {
//                byte[] dataCast = (byte[])data;
//                return true;
//            }
//            catch (Exception)
//            {
//                return false;
//            }
//        }
//    }

//    class CBORItemTag : ItemTag
//    {
//        public static ulong[] TAG_NUM = new ulong[] { 24 };

//        public CBORItemTag(ulong tagNum)
//        {
//            this.tagNumber = tagNum;
//        }
//        public override object processData(object data)
//        {
//            CBORDecoder decoder = new CBORDecoder((byte[])data);
//            return decoder.ReadItem();
//        }

//        public override bool isDataSupported(object data)
//        {
//            return (data is byte[]);
//        }
//    }

//    public class UnknownTag : ItemTag
//    {

//        public UnknownTag(ulong tagId)
//        {
//            this.tagNumber = tagId;
//        }

//        public override object processData(object data)
//        {
//            return data;
//        }

//        public override bool isDataSupported(object data)
//        {
//            return true;
//        }
//    }

//    class UriTag : ItemTag
//    {
//        public static ulong[] TAG_NUM = new ulong[] { 32 };
//        public UriTag(ulong tagNum)
//        {
//            this.tagNumber = tagNum;
//        }
//        public override object processData(object data)
//        {
//            return new Uri((data as string));
//        }

//        public override bool isDataSupported(object data)
//        {
//            return (data is string);
//        }
//    }
//}
