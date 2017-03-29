using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class UriUtil
    {
        public string EncodeUriComponent(string s, PNOperationType type, bool ignoreComma, bool ignorePercent2fEncode)
        {
            string encodedUri = "";
            bool prevSurroagePair = false;
            StringBuilder o = new StringBuilder();
            foreach (char ch in s)
            {
                if (prevSurroagePair)
                {
                    prevSurroagePair = false;
                    continue;
                }

                if (IsUnsafe(ch, ignoreComma))
                {
                    o.Append('%');
                    o.Append(ToHex(ch / 16));
                    o.Append(ToHex(ch % 16));
                }
                else
                {
                    int positionOfChar = s.IndexOf(ch);
                    if (ch == ',' && ignoreComma)
                    {
                        o.Append(ch.ToString());
                    }
                    else if (Char.IsSurrogatePair(s, positionOfChar))
                    {
                        string codepoint = ConvertToUtf32(s, positionOfChar).ToString("X4");

                        int codePointValue = int.Parse(codepoint, NumberStyles.HexNumber);
                        if (codePointValue <= 0x7F)
                        {
                            System.Diagnostics.Debug.WriteLine("0x7F");
                            string utf8HexValue = string.Format("%{0}", codePointValue);
                            o.Append(utf8HexValue);
                        }
                        else if (codePointValue <= 0x7FF)
                        {
                            string one = (0xC0 | ((codePointValue >> 6) & 0x1F)).ToString("X");
                            string two = (0x80 | (codePointValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}", one, two);
                            o.Append(utf8HexValue);
                        }
                        else if (codePointValue <= 0xFFFF)
                        {
                            string one = (0xE0 | ((codePointValue >> 12) & 0x0F)).ToString("X");
                            string two = (0x80 | ((codePointValue >> 6) & 0x3F)).ToString("X");
                            string three = (0x80 | (codePointValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}%{2}", one, two, three);
                            o.Append(utf8HexValue);
                        }
                        else if (codePointValue <= 0x10FFFF)
                        {
                            string one = (0xF0 | ((codePointValue >> 18) & 0x07)).ToString("X");
                            string two = (0x80 | ((codePointValue >> 12) & 0x3F)).ToString("X");
                            string three = (0x80 | ((codePointValue >> 6) & 0x3F)).ToString("X");
                            string four = (0x80 | (codePointValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}%{2}%{3}", one, two, three, four);
                            o.Append(utf8HexValue);
                        }

                        prevSurroagePair = true;
                    }
                    else
                    {
                        string escapeChar = System.Uri.EscapeDataString(ch.ToString());
                        o.Append(escapeChar);
                    }
                }
            }

            encodedUri = o.ToString();
            if (type == PNOperationType.PNHereNowOperation || type == PNOperationType.PNHistoryOperation || type == PNOperationType.Leave || type == PNOperationType.PNHeartbeatOperation || type == PNOperationType.PushRegister || type == PNOperationType.PushRemove || type == PNOperationType.PushGet || type == PNOperationType.PushUnregister)
            {
                if (!ignorePercent2fEncode)
                {
                    encodedUri = encodedUri.Replace("%2F", "%252F");
                }
            }

            return encodedUri;
        }

        private bool IsUnsafe(char ch, bool ignoreComma)
        {
            if (ignoreComma)
            {
                return " ~`!@#$%^&*()+=[]\\{}|;':\"/<>?".IndexOf(ch) >= 0;
            }
            else
            {
                return " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?".IndexOf(ch) >= 0;
            }
        }

        private char ToHex(int ch)
        {
            return (char)(ch < 10 ? '0' + ch : 'A' + ch - 10);
        }

        internal const int HighSurrogateStart = 0x00d800;
        internal const int LowSurrogateEnd = 0x00dfff;
        internal const int LowSurrogateStart = 0x00dc00;
        internal const int UnicodePlane01Start = 0x10000;

        private static int ConvertToUtf32(String s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (index < 0 || index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            //Contract.EndContractBlock();
            // Check if the character at index is a high surrogate.
            int temp1 = (int)s[index] - HighSurrogateStart;
            if (temp1 >= 0 && temp1 <= 0x7ff)
            {
                // Found a surrogate char.
                if (temp1 <= 0x3ff)
                {
                    // Found a high surrogate.
                    if (index < s.Length - 1)
                    {
                        int temp2 = (int)s[index + 1] - LowSurrogateStart;
                        if (temp2 >= 0 && temp2 <= 0x3ff)
                        {
                            // Found a low surrogate.
                            return (temp1 * 0x400) + temp2 + UnicodePlane01Start;
                        }
                        else
                        {
                            throw new ArgumentException("index");
                        }
                    }
                    else
                    {
                        // Found a high surrogate at the end of the string.
                        throw new ArgumentException("index");
                    }
                }
                else
                {
                    // Find a low surrogate at the character pointed by index.
                    throw new ArgumentException("index");
                }
            }

            // Not a high-surrogate or low-surrogate. Genereate the UTF32 value for the BMP characters.
            return (int)s[index];
        }


    }
}
