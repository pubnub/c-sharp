using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PubnubApi
{
    /// <summary>
    /// A simple not-all-covering implementation of CBOR serialisation/deserialisation for internal Pubnub ParseToken() usage
    /// Converted to C# from https://github.com/seba-aln/CBORCodec
    /// </summary>
    internal static class CBOR
    {
        private const byte TYPE_MASK = 0b11100000;
        private const byte ADDITIONAL_MASK = 0b00011111;

        public const byte TYPE_UNSIGNED_INT = 0b00000000;
        public const byte TYPE_NEGATIVE_INT = 0b00100000;
        public const byte TYPE_BYTE_STRING = 0b01000000;
        public const byte TYPE_TEXT_STRING = 0b01100000;
        public const byte TYPE_ARRAY = 0b10000000;
        public const byte TYPE_HASHMAP = 0b10100000;
        public const byte TYPE_FLOAT = 0b11100000;

        private const byte ADDITIONAL_LENGTH_1B = 24;
        private const byte ADDITIONAL_LENGTH_2B = 25;
        private const byte ADDITIONAL_LENGTH_4B = 26;
        private const byte ADDITIONAL_LENGTH_8B = 27;

        private const byte ADDITIONAL_TYPE_INDEFINITE = 31;

        private const byte INDEFINITE_BREAK = 0b11111111;

        private const byte ADDITIONAL_SIMPLE_FALSE = 20;
        private const byte ADDITIONAL_SIMPLE_TRUE = 21;
        private const byte ADDITIONAL_SIMPLE_NULL = 22;
        private const byte ADDITIONAL_SIMPLE_UNDEF = 23;

        private const int MAX_NESTING_DEPTH = 64;

        private static readonly Dictionary<byte, int> additionalLengthBytes = new Dictionary<byte, int>
        {
            { ADDITIONAL_LENGTH_1B, 1 },
            { ADDITIONAL_LENGTH_2B, 2 },
            { ADDITIONAL_LENGTH_4B, 4 },
            { ADDITIONAL_LENGTH_8B, 8 },
        };

        private const string SIMPLE_VALUE_FALSE = "F4";
        private const string SIMPLE_VALUE_TRUE = "F5";
        private const string SIMPLE_VALUE_NULL = "F6";
        private const string SIMPLE_VALUE_UNDEF = "F7";

        /// <summary>
        /// Decode incoming hexadecimal string of data and outputing decoded values
        /// </summary>
        /// <param name="value">Hexadecimal string to decode</param>
        /// <returns>Decoded value</returns>
        /// <exception cref="Exception">Thrown when input is invalid or unsupported type</exception>
        public static object Decode(string value)
        {
            value = SanitizeInput(value);
            if (value.Length % 2 != 0)
            {
                throw new Exception("Invalid Input");
            }
            return Decode(HexToBytes(value));
        }

        /// <summary>
        /// Decode CBOR encoded bytes and output decoded values
        /// </summary>
        /// <param name="data">CBOR encoded bytes to decode</param>
        /// <returns>Decoded value</returns>
        /// <exception cref="Exception">Thrown when input is invalid, nested too deeply or of unsupported type</exception>
        public static object Decode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            var position = 0;
            return ParseData(data, ref position, 0);
        }

        private static object ParseData(byte[] data, ref int position, int depth)
        {
            var initialByte = ReadByte(data, ref position);
            var type = (byte)(initialByte & TYPE_MASK);
            var additional = (byte)(initialByte & ADDITIONAL_MASK);

            switch (type)
            {
                case TYPE_UNSIGNED_INT:
                    var unsignedValue = ReadArgument(data, ref position, additional);
                    return unsignedValue <= long.MaxValue ? (object)(long)unsignedValue : unsignedValue;

                case TYPE_NEGATIVE_INT:
                    var negativeValue = ReadArgument(data, ref position, additional);
                    if (negativeValue > long.MaxValue)
                    {
                        throw new Exception("Negative integer out of supported range");
                    }
                    return -1 - (long)negativeValue;

                case TYPE_FLOAT:
                    switch (additional)
                    {
                        case ADDITIONAL_SIMPLE_FALSE:
                            return false;
                        case ADDITIONAL_SIMPLE_TRUE:
                            return true;
                        case ADDITIONAL_SIMPLE_NULL:
                        case ADDITIONAL_SIMPLE_UNDEF:
                            return null;
                        case ADDITIONAL_LENGTH_1B:
                            var simpleValue = ReadByte(data, ref position);
                            if (simpleValue < 32)
                            {
                                throw new Exception($"Invalid simple value {simpleValue}");
                            }
                            return simpleValue;
                        case ADDITIONAL_LENGTH_2B:
                        case ADDITIONAL_LENGTH_4B:
                        case ADDITIONAL_LENGTH_8B:
                            return DecodeFloat(ReadArgument(data, ref position, additional), additional);
                        case ADDITIONAL_TYPE_INDEFINITE:
                            throw new Exception("Unexpected break");
                        default:
                            if (additional < ADDITIONAL_SIMPLE_FALSE)
                            {
                                return additional;
                            }
                            throw new Exception($"Unsupported additional information {additional}");
                    }

                case TYPE_BYTE_STRING:
                    // For byte strings, return the raw bytes
                    return ReadStringBytes(data, ref position, type, additional);

                case TYPE_TEXT_STRING:
                    // For text strings, convert to UTF-8 string
                    return Encoding.UTF8.GetString(ReadStringBytes(data, ref position, type, additional));

                case TYPE_ARRAY:
                    EnsureNestingDepth(depth);
                    var arrayResult = new List<object>();
                    if (additional == ADDITIONAL_TYPE_INDEFINITE)
                    {
                        while (!TryReadBreak(data, ref position))
                        {
                            arrayResult.Add(ParseData(data, ref position, depth + 1));
                        }
                    }
                    else
                    {
                        var arrayLength = ReadLength(data, ref position, additional, 1);
                        for (int i = 0; i < arrayLength; i++)
                        {
                            arrayResult.Add(ParseData(data, ref position, depth + 1));
                        }
                    }
                    return arrayResult;

                case TYPE_HASHMAP:
                    EnsureNestingDepth(depth);
                    var hashmapResult = new Dictionary<object, object>();
                    if (additional == ADDITIONAL_TYPE_INDEFINITE)
                    {
                        while (!TryReadBreak(data, ref position))
                        {
                            ParseHashmapEntry(data, ref position, depth + 1, hashmapResult);
                        }
                    }
                    else
                    {
                        var hashmapLength = ReadLength(data, ref position, additional, 2);
                        for (int i = 0; i < hashmapLength; i++)
                        {
                            ParseHashmapEntry(data, ref position, depth + 1, hashmapResult);
                        }
                    }
                    return hashmapResult;

                default:
                    throw new Exception($"Unsupported Type {Convert.ToString(type, 2)}");
            }
        }

        private static void ParseHashmapEntry(byte[] data, ref int position, int depth, Dictionary<object, object> hashmap)
        {
            var key = ParseData(data, ref position, depth);
            var val = ParseData(data, ref position, depth);
            hashmap[key] = val;
        }

        private static void EnsureNestingDepth(int depth)
        {
            if (depth >= MAX_NESTING_DEPTH)
            {
                throw new Exception($"Maximum nesting depth of {MAX_NESTING_DEPTH} exceeded");
            }
        }

        private static double DecodeFloat(ulong bits, byte precision)
        {
            switch (precision)
            {
                case ADDITIONAL_LENGTH_2B:
                    var sign = (bits & 0b1000000000000000) >> 15;
                    var exp = (int)((bits & 0b0111110000000000) >> 10);
                    var mant = (int)(bits & 0b1111111111);
                    double result;
                    if (exp == 0)
                    {
                        result = Math.Pow(2, -14) * (mant / 1024.0);
                    }
                    else if (exp == 0b11111)
                    {
                        result = mant == 0 ? double.PositiveInfinity : double.NaN;
                    }
                    else
                    {
                        result = Math.Pow(2, exp - 15) * (1 + mant / 1024.0);
                    }
                    return sign == 1 ? -result : result;

                case ADDITIONAL_LENGTH_4B:
                    return BitConverter.ToSingle(BitConverter.GetBytes((uint)bits), 0);

                case ADDITIONAL_LENGTH_8B:
                    return BitConverter.Int64BitsToDouble((long)bits);

                default:
                    throw new Exception($"Unsupported float precision: {precision}");
            }
        }

        private static byte ReadByte(byte[] data, ref int position)
        {
            EnsureAvailable(data, position, 1);
            return data[position++];
        }

        private static bool TryReadBreak(byte[] data, ref int position)
        {
            EnsureAvailable(data, position, 1);
            if (data[position] != INDEFINITE_BREAK)
            {
                return false;
            }
            position++;
            return true;
        }

        private static void EnsureAvailable(byte[] data, int position, int count)
        {
            if (data.Length - position < count)
            {
                throw new Exception("Unexpected end of data");
            }
        }

        /// <summary>
        /// Reads the unsigned argument encoded in the additional information of the initial byte and the bytes following it
        /// </summary>
        private static ulong ReadArgument(byte[] data, ref int position, byte additional)
        {
            if (additional < ADDITIONAL_LENGTH_1B)
            {
                return additional;
            }
            if (!additionalLengthBytes.TryGetValue(additional, out var byteCount))
            {
                throw new Exception($"Unsupported additional information {additional}");
            }
            EnsureAvailable(data, position, byteCount);
            ulong value = 0;
            for (int i = 0; i < byteCount; i++)
            {
                value = (value << 8) | data[position++];
            }
            return value;
        }

        /// <summary>
        /// Reads a declared length and rejects it when the remaining data cannot hold that many items
        /// </summary>
        private static int ReadLength(byte[] data, ref int position, byte additional, int minimumBytesPerItem)
        {
            var length = ReadArgument(data, ref position, additional);
            var remaining = (ulong)(data.Length - position);
            if (length > remaining / (ulong)minimumBytesPerItem)
            {
                throw new Exception($"Declared length {length} exceeds remaining data");
            }
            return (int)length;
        }

        private static byte[] ReadStringBytes(byte[] data, ref int position, byte type, byte additional)
        {
            if (additional != ADDITIONAL_TYPE_INDEFINITE)
            {
                var length = ReadLength(data, ref position, additional, 1);
                var result = new byte[length];
                Buffer.BlockCopy(data, position, result, 0, length);
                position += length;
                return result;
            }

            using (var chunks = new MemoryStream())
            {
                while (!TryReadBreak(data, ref position))
                {
                    var chunkHeader = ReadByte(data, ref position);
                    var chunkAdditional = (byte)(chunkHeader & ADDITIONAL_MASK);
                    if ((chunkHeader & TYPE_MASK) != type || chunkAdditional == ADDITIONAL_TYPE_INDEFINITE)
                    {
                        throw new Exception("Invalid chunk in indefinite-length string");
                    }
                    var chunkLength = ReadLength(data, ref position, chunkAdditional, 1);
                    chunks.Write(data, position, chunkLength);
                    position += chunkLength;
                }
                return chunks.ToArray();
            }
        }

        /// <summary>
        /// Removes spaces, converts string to upper case and throws exception if input is not a valid hexadecimal string
        /// </summary>
        /// <param name="value">Input string to sanitize</param>
        /// <returns>Sanitized hexadecimal string</returns>
        /// <exception cref="Exception">Thrown when input contains invalid characters</exception>
        private static string SanitizeInput(string value)
        {
            value = value.Replace(" ", "").ToUpperInvariant();
            if (!Regex.IsMatch(value, "^[A-F0-9]*$"))
            {
                throw new Exception("Invalid Input");
            }
            return value;
        }

        /// <summary>
        /// Sanitizes the output value so it contains even number of characters and returns it upper cased
        /// </summary>
        /// <param name="value">Hexadecimal value to sanitize</param>
        /// <param name="useByteLength">Should the length of output be in powers of two (2, 4, 8, 16)</param>
        /// <returns>Sanitized hexadecimal string</returns>
        private static string SanitizeOutput(string value, bool useByteLength = false)
        {
            value = value.ToUpperInvariant();
            var length = value.Length;

            if (useByteLength)
            {
                if (length == 1 || length == 3)
                {
                    value = "0" + value;
                }
                else if (length > 4 && length < 8)
                {
                    value = value.PadLeft(8, '0');
                }
                else if (length > 8 && length < 16)
                {
                    value = value.PadLeft(16, '0');
                }
            }
            else if (length % 2 == 1)
            {
                value = "0" + value;
            }

            return value;
        }

        /// <summary>
        /// Encodes value to a hexadecimal CBOR string. Because C# does not differentiate byte strings and text strings
        /// the only way to manipulate output type of strings is to pass a string type (one of CBOR::TYPE_TEXT_STRING and
        /// CBOR::TYPE_BYTE_STRING).
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <param name="stringType">Type of string encoding</param>
        /// <returns>Hexadecimal CBOR string</returns>
        /// <exception cref="Exception">Thrown when unsupported type is passed</exception>
        public static string Encode(object value, byte stringType = TYPE_TEXT_STRING)
        {
            if (value == null)
            {
                return SIMPLE_VALUE_NULL;
            }

            switch (value)
            {
                case bool boolValue:
                    return boolValue ? SIMPLE_VALUE_TRUE : SIMPLE_VALUE_FALSE;

                case long:
                case int:
                    var numValue = Convert.ToInt64(value);
                    var type = TYPE_UNSIGNED_INT;
                    if (numValue < 0)
                    {
                        type = TYPE_NEGATIVE_INT;
                        numValue = Math.Abs(numValue + 1);
                    }
                    if (numValue <= 23)
                    {
                        return SanitizeOutput(Convert.ToString(type | numValue, 16));
                    }
                    else
                    {
                        var hexValue = SanitizeOutput(Convert.ToString(numValue, 16), true);
                        var lengthHeader = additionalLengthBytes.FirstOrDefault(x => x.Value == hexValue.Length / 2).Key;
                        var header = SanitizeOutput(Convert.ToString(type | lengthHeader, 16));
                        return header + hexValue;
                    }

                case string stringValue:
                    var strType = stringType;
                    var hexString = SanitizeOutput(BinaryToHex(Encoding.UTF8.GetBytes(stringValue)));
                    var strLength = hexString.Length / 2;
                    var strHeader = BuildHeader(strType, strLength);
                    return strHeader + hexString;

                case double doubleValue:
                    var floatHeader = Convert.ToString(TYPE_FLOAT | ADDITIONAL_LENGTH_8B, 16);
                    var floatBytes = BitConverter.GetBytes(doubleValue);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(floatBytes);
                    }
                    var floatHex = BitConverter.ToString(floatBytes).Replace("-", "");
                    return floatHeader + floatHex;

                case List<object> listValue:
                    var listLength = listValue.Count;
                    var listType = TYPE_ARRAY;
                    var listResult = BuildHeader(listType, listLength);
                    foreach (var element in listValue)
                    {
                        listResult += Encode(element, stringType);
                    }
                    return SanitizeOutput(listResult);

                case Dictionary<object, object> dictValue:
                    var dictLength = dictValue.Count;
                    var dictType = TYPE_HASHMAP;
                    var dictResult = BuildHeader(dictType, dictLength);
                    foreach (var kvp in dictValue)
                    {
                        dictResult += Encode(kvp.Key, stringType);
                        dictResult += Encode(kvp.Value, stringType);
                    }
                    return SanitizeOutput(dictResult);

                default:
                    throw new Exception($"Unsupported type passed to encoding: {value.GetType().Name}");
            }
        }

        private static string BuildHeader(byte type, long length)
        {
            if (length > 0xffffffffffff)
            {
                var header = Convert.ToString(type | ADDITIONAL_TYPE_INDEFINITE, 16);
                var footer = Convert.ToString(INDEFINITE_BREAK, 16);
                return header + footer;
            }
            else if (length > 0xffffffff)
            {
                var header = Convert.ToString(type | ADDITIONAL_LENGTH_8B, 16) + SanitizeOutput(Convert.ToString(length, 16));
                return header;
            }
            else if (length > 0xffff)
            {
                var header = Convert.ToString(type | ADDITIONAL_LENGTH_4B, 16) + SanitizeOutput(Convert.ToString(length, 16));
                return header;
            }
            else if (length > 0xff)
            {
                var header = Convert.ToString(type | ADDITIONAL_LENGTH_2B, 16) + SanitizeOutput(Convert.ToString(length, 16));
                return header;
            }
            else if (length > 23)
            {
                var header = Convert.ToString(type | ADDITIONAL_LENGTH_1B, 16) + SanitizeOutput(Convert.ToString(length, 16));
                return header;
            }
            else
            {
                var header = Convert.ToString(type | length, 16);
                return header;
            }
        }

        private static byte[] HexToBytes(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static string BinaryToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
} 