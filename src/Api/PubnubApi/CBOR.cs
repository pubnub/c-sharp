using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PubnubApi
{
    /// <summary>
    /// A simple not-all-covering implementation of CBOR serialisation/deserialisation for internal Pubnub ParseToken() usage
    /// Converted to C# from https://github.com/seba-aln/CBORCodec
    /// </summary>
    internal class CBOR
    {
        private const byte TYPE_MASK = 0b11100000;
        private const byte ADDITIONAL_MASK = 0b00011111;

        public const byte TYPE_UNSIGNED_INT = 0b00000000;
        public const byte TYPE_NEGATIVE_INT = 0b00100000;
        public const byte TYPE_BYTE_STRING = 0b01000000;
        public const byte TYPE_TEXT_STRING = 0b01100000;
        public const byte TYPE_ARRAY = 0b10000000;
        public const byte TYPE_HASHMAP = 0b10100000;
        public const byte TYPE_TAG = 0b11000000;
        public const byte TYPE_FLOAT = 0b11100000;

        private const byte ADDITIONAL_LENGTH_1B = 24;
        private const byte ADDITIONAL_LENGTH_2B = 25;
        private const byte ADDITIONAL_LENGTH_4B = 26;
        private const byte ADDITIONAL_LENGTH_8B = 27;

        private const byte ADDITIONAL_TYPE_INDEFINITE = 31;

        private const byte INDEFINITE_BREAK = 0b11111111;

        private static readonly byte[] additionalLength = {
            ADDITIONAL_LENGTH_1B,
            ADDITIONAL_LENGTH_2B,
            ADDITIONAL_LENGTH_4B,
            ADDITIONAL_LENGTH_8B,
        };

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

        private static readonly Dictionary<string, object> simpleValues = new Dictionary<string, object>
        {
            { SIMPLE_VALUE_FALSE, false },
            { SIMPLE_VALUE_TRUE, true },
            { SIMPLE_VALUE_NULL, null },
            { SIMPLE_VALUE_UNDEF, null }
        };

        /// <summary>
        /// Decode incoming hexadecimal string of data and outputing decoded values
        /// </summary>
        /// <param name="value">Hexadecimal string to decode</param>
        /// <returns>Decoded value</returns>
        /// <exception cref="Exception">Thrown when input is invalid or unsupported type</exception>
        public static object Decode(string value)
        {
            value = SanitizeInput(value);
            var data = SplitIntoBytes(value);
            return ParseData(ref data);
        }

        private static object ParseData(ref List<string> data)
        {
            if (data.Count == 0)
                throw new Exception("Unexpected end of data");

            var byteStr = data[0];
            data.RemoveAt(0);

            if (simpleValues.ContainsKey(byteStr))
            {
                return simpleValues[byteStr];
            }

            var byteValue = Convert.ToByte(byteStr, 16);
            var type = (byte)(byteValue & TYPE_MASK);
            var additional = (byte)(byteValue & ADDITIONAL_MASK);

            switch (type)
            {
                case TYPE_NEGATIVE_INT:
                case TYPE_UNSIGNED_INT:
                    long value;
                    if (additionalLength.Contains(additional))
                    {
                        value = Convert.ToInt64(GetData(ref data, additionalLengthBytes[additional]), 16);
                    }
                    else
                    {
                        value = additional;
                    }
                    if (type == TYPE_NEGATIVE_INT)
                    {
                        value = -1 - value;
                    }
                    return value;

                case TYPE_FLOAT:
                    if (additional <= 23)
                    {
                        return additional;
                    }
                    else if (additional == ADDITIONAL_LENGTH_1B)
                    {
                        return GetData(ref data);
                    }
                    else
                    {
                        return DecodeFloat(GetData(ref data, additionalLengthBytes[additional]), additional);
                    }

                case TYPE_BYTE_STRING:
                    // For byte strings, return the raw bytes
                    byte[] resultBytes;
                    if (additionalLength.Contains(additional))
                    {
                        var length = Convert.ToInt32(GetData(ref data, additionalLengthBytes[additional]), 16);
                        resultBytes = HexToBytes(GetData(ref data, length));
                    }
                    else if (additional == ADDITIONAL_TYPE_INDEFINITE)
                    {
                        resultBytes = HexToBytes(GetIndefiniteData(ref data));
                    }
                    else
                    {
                        resultBytes = HexToBytes(GetData(ref data, additional));
                    }
                    return resultBytes;

                case TYPE_TEXT_STRING:
                    // For text strings, convert to UTF-8 string
                    string result;
                    if (additionalLength.Contains(additional))
                    {
                        var length = Convert.ToInt32(GetData(ref data, additionalLengthBytes[additional]), 16);
                        result = HexToBinary(GetData(ref data, length));
                    }
                    else if (additional == ADDITIONAL_TYPE_INDEFINITE)
                    {
                        result = HexToBinary(GetIndefiniteData(ref data));
                    }
                    else
                    {
                        result = HexToBinary(GetData(ref data, additional));
                    }
                    return result;

                case TYPE_ARRAY:
                    var arrayResult = new List<object>();
                    int arrayLength;
                    if (additionalLength.Contains(additional))
                    {
                        arrayLength = Convert.ToInt32(GetData(ref data, additionalLengthBytes[additional]), 16);
                    }
                    else
                    {
                        arrayLength = additional;
                    }

                    for (int i = 0; i < arrayLength; i++)
                    {
                        arrayResult.Add(ParseData(ref data));
                    }
                    return arrayResult;

                case TYPE_HASHMAP:
                    var hashmapResult = new Dictionary<object, object>();
                    int hashmapLength;
                    if (additionalLength.Contains(additional))
                    {
                        hashmapLength = Convert.ToInt32(GetData(ref data, additionalLengthBytes[additional]), 16);
                    }
                    else
                    {
                        hashmapLength = additional;
                    }

                    for (int i = 0; i < hashmapLength; i++)
                    {
                        var key = ParseData(ref data);
                        var val = ParseData(ref data);
                        hashmapResult[key] = val;
                    }
                    return hashmapResult;

                default:
                    throw new Exception($"Unsupported Type {Convert.ToString(type, 2)}");
            }
        }

        private static double DecodeFloat(string value, byte precision)
        {
            var bytes = Convert.ToUInt64(value, 16);
            switch (precision)
            {
                case ADDITIONAL_LENGTH_2B:
                    var sign = (bytes & 0b1000000000000000) >> 15;
                    var exp = (bytes & 0b0111110000000000) >> 10;
                    var mant = bytes & 0b1111111111;
                    double result;
                    if (exp == 0)
                    {
                        result = Math.Pow(2, -14) * (mant / 1024.0);
                    }
                    else if (exp == 0b11111)
                    {
                        result = double.PositiveInfinity;
                    }
                    else
                    {
                        result = Math.Pow(2, exp - 15) * (1 + mant / 1024.0);
                    }
                    return (sign == 1 ? -1 : 1) * result;

                case ADDITIONAL_LENGTH_4B:
                    var sign32 = (bytes >> 31) == 1 ? -1 : 1;
                    var x = (bytes & ((1UL << 23) - 1)) + (1UL << 23) * ((bytes >> 31) | 1);
                    var exp32 = (int)((bytes >> 23) & 0xFF) - 127;
                    return x * Math.Pow(2, exp32 - 23) * sign32;

                case ADDITIONAL_LENGTH_8B:
                    var sign64 = (bytes >> 63) == 1 ? -1 : 1;
                    var exp64 = (bytes >> 52) & 0x7ff;
                    var mant64 = bytes & 0xfffffffffffff;

                    double val;
                    if (exp64 == 0)
                    {
                        val = mant64 * Math.Pow(2, -(1022 + 52));
                    }
                    else if (exp64 != 0b11111111111)
                    {
                        val = (mant64 + (1UL << 52)) * Math.Pow(2, exp64 - (1023 + 52));
                    }
                    else
                    {
                        val = mant64 == 0 ? double.PositiveInfinity : double.NaN;
                    }
                    return sign64 * val;

                default:
                    throw new Exception($"Unsupported float precision: {precision}");
            }
        }

        private static string GetData(ref List<string> data, int bytes = 1)
        {
            var result = new StringBuilder();
            for (int i = 1; i <= bytes; i++)
            {
                if (data.Count == 0)
                    throw new Exception("Unexpected end of data");
                result.Append(data[0]);
                data.RemoveAt(0);
            }
            return result.ToString();
        }

        private static string GetIndefiniteData(ref List<string> data)
        {
            var result = new StringBuilder();
            do
            {
                if (data.Count == 0)
                    throw new Exception("Unexpected end of data");
                var byteStr = data[0];
                data.RemoveAt(0);
                if (Convert.ToByte(byteStr, 16) == INDEFINITE_BREAK)
                {
                    break;
                }
                result.Append(byteStr);
            } while (data.Count > 0);
            return result.ToString();
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

                case long longValue:
                case int intValue:
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

        private static List<string> SplitIntoBytes(string hexString)
        {
            var result = new List<string>();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                if (i + 1 < hexString.Length)
                {
                    result.Add(hexString.Substring(i, 2));
                }
                else
                {
                    result.Add(hexString.Substring(i, 1));
                }
            }
            return result;
        }

        private static string HexToBinary(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return Encoding.UTF8.GetString(bytes);
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