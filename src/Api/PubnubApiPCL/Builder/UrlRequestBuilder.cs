using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Globalization;

using System.Reflection;

namespace PubnubApi
{
    public class UrlRequestBuilder : IUrlRequestBuilder
    {
        private PNConfiguration _config;

        public UrlRequestBuilder(PNConfiguration config)
        {
            this._config = config;
        }

        Uri IUrlRequestBuilder.BuildTimeRequest()
        {
            List<string> url = new List<string>();

            url.Add("time");
            url.Add("0");

            return BuildRestApiRequest<Uri>(url, ResponseType.Time);
        }

        private Uri BuildRestApiRequest<T>(List<string> urlComponents, ResponseType type)
        {
            return BuildRestApiRequest<T>(urlComponents, type, this._config.Uuid);
        }

        private Uri BuildRestApiRequest<T>(List<string> urlComponents, ResponseType type, string uuid)
        {
            bool queryParamExist = false;
            StringBuilder url = new StringBuilder();

            uuid = EncodeUricomponent(uuid, type, false, false);

            // Add http or https based on SSL flag
            if (_config.Secure)
            {
                url.Append("https://");
            }
            else
            {
                url.Append("http://");
            }

            // Add Origin To The Request
            url.Append(_config.Origin);

            // Generate URL with UTF-8 Encoding
            for (int componentIndex = 0; componentIndex < urlComponents.Count; componentIndex++)
            {
                url.Append("/");

                if (type == ResponseType.Publish && componentIndex == urlComponents.Count - 1)
                {
                    url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, false, false));
                }
                else
                {
                    url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, true, false));
                }
            }

            if (type == ResponseType.Presence || type == ResponseType.Subscribe || type == ResponseType.Leave)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //url.Append(subscribeParameters);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //if (_pubnubPresenceHeartbeatInSeconds != 0)
                //{
                //    url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.PresenceHeartbeat)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //url.Append(presenceHeartbeatParameters);
                //if (_pubnubPresenceHeartbeatInSeconds != 0)
                //{
                //    url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
                //}
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.SetUserState)
            {
                //queryParamExist = true;
                //url.Append(setUserStateParameters);
                //url.AppendFormat("&uuid={0}", uuid);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.GetUserState)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //url.Append(getUserStateParameters);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));

            }
            else if (type == ResponseType.Here_Now)
            {
                //queryParamExist = true;
                //url.Append(hereNowParameters);
                //url.AppendFormat("&uuid={0}", uuid);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.GlobalHere_Now)
            {
                //queryParamExist = true;
                //url.Append(globalHereNowParameters);
                //url.AppendFormat("&uuid={0}", uuid);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.Where_Now)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.Publish)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //if (parameters != "")
                //{
                //    url.AppendFormat("&{0}", parameters);
                //}
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
            {
                //queryParamExist = true;
                //switch (type)
                //{
                //    case ResponseType.PushRegister:
                //        url.Append(pushRegisterDeviceParameters);
                //        break;
                //    case ResponseType.PushRemove:
                //        url.Append(pushRemoveChannelParameters);
                //        break;
                //    case ResponseType.PushUnregister:
                //        url.Append(pushUnregisterDeviceParameters);
                //        break;
                //    default:
                //        url.Append(pushGetChannelsParameters);
                //        break;
                //}
                //url.AppendFormat("&uuid={0}", uuid);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.ChannelGroupAdd || type == ResponseType.ChannelGroupRemove || type == ResponseType.ChannelGroupGet)
            {
                //queryParamExist = true;
                //switch (type)
                //{
                //    case ResponseType.ChannelGroupAdd:
                //        url.Append(channelGroupAddParameters);
                //        break;
                //    case ResponseType.ChannelGroupRemove:
                //        url.Append(channelGroupRemoveParameters);
                //        break;
                //    case ResponseType.ChannelGroupGet:
                //        break;
                //    default:
                //        break;
                //}
            }
            else if (type == ResponseType.DetailedHistory
                || type == ResponseType.GrantAccess || type == ResponseType.AuditAccess || type == ResponseType.RevokeAccess
                || type == ResponseType.ChannelGroupGrantAccess || type == ResponseType.ChannelGroupAuditAccess || type == ResponseType.ChannelGroupRevokeAccess)
            {
                //url.Append(parameters);
                //queryParamExist = true;
            }

            if (!queryParamExist)
            {
                url.AppendFormat("?uuid={0}", uuid);
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_config.SdkVersion, type, false, true));
            }


            Uri requestUri = new Uri(url.ToString());

            if (type == ResponseType.Publish || type == ResponseType.Subscribe || type == ResponseType.Presence)
            {
                ForceCanonicalPathAndQuery(requestUri);
            }

            return requestUri;

        }

        private string EncodeUricomponent(string s, ResponseType type, bool ignoreComma, bool ignorePercent2fEncode)
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

                        int cpValue = int.Parse(codepoint, NumberStyles.HexNumber);
                        if (cpValue <= 0x7F)
                        {
                            System.Diagnostics.Debug.WriteLine("0x7F");
                            string utf8HexValue = string.Format("%{0}", cpValue);
                            o.Append(utf8HexValue);
                        }
                        else if (cpValue <= 0x7FF)
                        {
                            string one = (0xC0 | ((cpValue >> 6) & 0x1F)).ToString("X");
                            string two = (0x80 | ((cpValue) & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}", one, two);
                            o.Append(utf8HexValue);
                        }
                        else if (cpValue <= 0xFFFF)
                        {
                            string one = (0xE0 | ((cpValue >> 12) & 0x0F)).ToString("X");
                            string two = (0x80 | ((cpValue >> 6) & 0x3F)).ToString("X");
                            string three = (0x80 | ((cpValue) & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}%{2}", one, two, three);
                            o.Append(utf8HexValue);
                        }
                        else if (cpValue <= 0x10FFFF)
                        {
                            string one = (0xF0 | ((cpValue >> 18) & 0x07)).ToString("X");
                            string two = (0x80 | ((cpValue >> 12) & 0x3F)).ToString("X");
                            string three = (0x80 | ((cpValue >> 6) & 0x3F)).ToString("X");
                            string four = (0x80 | ((cpValue) & 0x3F)).ToString("X");
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
            if (type == ResponseType.Here_Now || type == ResponseType.DetailedHistory || type == ResponseType.Leave || type == ResponseType.PresenceHeartbeat || type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
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

        internal const int HIGH_SURROGATE_START = 0x00d800;
        internal const int LOW_SURROGATE_END = 0x00dfff;
        internal const int LOW_SURROGATE_START = 0x00dc00;
        internal const int UNICODE_PLANE01_START = 0x10000;

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
            int temp1 = (int)s[index] - HIGH_SURROGATE_START;
            if (temp1 >= 0 && temp1 <= 0x7ff)
            {
                // Found a surrogate char.
                if (temp1 <= 0x3ff)
                {
                    // Found a high surrogate.
                    if (index < s.Length - 1)
                    {
                        int temp2 = (int)s[index + 1] - LOW_SURROGATE_START;
                        if (temp2 >= 0 && temp2 <= 0x3ff)
                        {
                            // Found a low surrogate.
                            return ((temp1 * 0x400) + temp2 + UNICODE_PLANE01_START);
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
            return ((int)s[index]);
        }

        private void ForceCanonicalPathAndQuery(Uri requestUri)
        {
            LoggingMethod.WriteToLog("Inside ForceCanonicalPathAndQuery = " + requestUri.ToString(), LoggingMethod.LevelInfo);
            try
            {
                FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
                if (flagsFieldInfo != null)
                {
                    ulong flags = (ulong)flagsFieldInfo.GetValue(requestUri);
                    flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
                    flagsFieldInfo.SetValue(requestUri, flags);
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog("Exception Inside ForceCanonicalPathAndQuery = " + ex.ToString(), LoggingMethod.LevelInfo);
            }
        }
    }
}
