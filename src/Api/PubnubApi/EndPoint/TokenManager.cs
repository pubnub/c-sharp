using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;
#if DEBUG && NET461
#endif
using Newtonsoft.Json;
using PeterO.Cbor;

namespace PubnubApi.EndPoint
{
    public class TokenManager : IDisposable
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;
        private readonly string pubnubInstanceId;
        private static ConcurrentDictionary<string, ConcurrentDictionary<PNTokenKey, string>> dicToken
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<PNTokenKey, string>>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Codacy.Sonar", "S1144:Unused private types or members should be removed")]
        internal class TokenManagerConverter : JsonConverter // NOSONAR
        {
            public override bool CanConvert(Type objectType)
            {
                return (typeof(IDictionary).IsAssignableFrom(objectType) ||
                        TypeImplementsGenericInterface(objectType, typeof(IDictionary<,>)));
            }

            private static bool TypeImplementsGenericInterface(Type concreteType, Type interfaceType)
            {
#if NET35 || NET40
                return concreteType.GetInterfaces()
                       .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
#else
                return concreteType.GetInterfaces()
                       .Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
#endif
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Type type = value.GetType();
                IEnumerable keys = (IEnumerable)type.GetProperty("Keys").GetValue(value, null);
                IEnumerable values = (IEnumerable)type.GetProperty("Values").GetValue(value, null);
                IEnumerator valueEnumerator = values.GetEnumerator();

                writer.WriteStartArray();
                foreach (object key in keys)
                {
                    valueEnumerator.MoveNext();

                    writer.WriteStartArray();
                    serializer.Serialize(writer, key);
                    serializer.Serialize(writer, valueEnumerator.Current);
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public TokenManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log, string instanceId)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubInstanceId = instanceId;
            if (!dicToken.ContainsKey(instanceId))
            {
                dicToken.GetOrAdd(instanceId, new ConcurrentDictionary<PNTokenKey, string>());
            }
            
        }

        private static string GetDisplayableBytes(byte[] currentBytes)
        {
            StringBuilder outBuilder = new StringBuilder("{ ");
            for (int di = 0; di < currentBytes.Length; di++)
            {
                outBuilder.Append(currentBytes[di]);
                if (di < currentBytes.Length - 1)
                {
                    outBuilder.Append(", ");
                }
            }
            outBuilder.Append(" }");
            return outBuilder.ToString();
        }

        public PNGrantToken ParseToken(string token)
        {
            PNGrantToken result = null;
            try
            {
                if (!string.IsNullOrEmpty(token) && token.Trim().Length > 0)
                {
                    string refinedToken = token.Replace('_', '/').Replace('-', '+');
                    byte[] tokenByteArray = Convert.FromBase64String(refinedToken);
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} IV = {1}", DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture), GetDisplayableBytes(tokenByteArray)), PNLogVerbosity.BODY);
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(tokenByteArray))
                    {
                        CBORObject cborObj = CBORObject.DecodeFromBytes(tokenByteArray);

                        if (cborObj != null)
                        {
                            result = new PNGrantToken();
                            result.Meta = new Dictionary<string, object>();
                            result.Channels = new Dictionary<string, PNResourcePermission>();
                            result.ChannelGroups = new Dictionary<string, PNResourcePermission>();
                            result.Users = new Dictionary<string, PNResourcePermission>();
                            result.Spaces = new Dictionary<string, PNResourcePermission>();
                            result.ChannelPatterns = new Dictionary<string, PNResourcePermission>();
                            result.GroupPatterns = new Dictionary<string, PNResourcePermission>();
                            result.UserPatterns = new Dictionary<string, PNResourcePermission>();
                            result.SpacePatterns = new Dictionary<string, PNResourcePermission>();
                            ParseCBOR(cborObj, "", ref result);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                if (this.pubnubLog != null && this.pubnubConfig != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, ex.ToString(), pubnubConfig.LogVerbosity);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            return result;
        }

        public void ParseCBOR(CBORObject cbor, string parent, ref PNGrantToken pnGrantTokenDecoded)
        {
            foreach (KeyValuePair<CBORObject, CBORObject> kvp in cbor.Entries)
            {
                if (kvp.Key.Type.ToString().Equals("ByteString"))
                {
#if NETSTANDARD10 || NETSTANDARD11
                    UTF8Encoding utf8 = new UTF8Encoding(true, true);
                    byte[] keyBytes = kvp.Key.GetByteString();
                    string key = utf8.GetString(keyBytes, 0, keyBytes.Length);
#else
                    string key = Encoding.ASCII.GetString(kvp.Key.GetByteString());
#endif
                    ParseCBORValue(key, parent, kvp, ref pnGrantTokenDecoded);
                }
                else if (kvp.Key.Type.ToString().Equals("TextString"))
                {
                    ParseCBORValue(kvp.Key.ToString(), parent, kvp, ref pnGrantTokenDecoded);
                }

            }
        }

        private void ParseCBORValue(string key, string parent, KeyValuePair<CBORObject, CBORObject> kvp, ref PNGrantToken pnGrantTokenDecoded)
        {
            if (kvp.Value.Type.ToString().Equals("Map"))
            {
                var p = string.Format("{0}{1}{2}", parent, string.IsNullOrEmpty(parent) ? "" : ":", key);
                ParseCBOR(kvp.Value, p, ref pnGrantTokenDecoded);
            }
            else if (kvp.Value.Type.ToString().Equals("ByteString"))
            {
                FillGrantToken(parent, key, kvp.Value, typeof(string), ref pnGrantTokenDecoded);
            }
            else if (kvp.Value.Type.ToString().Equals("Integer"))
            {
#if (ENABLE_PUBNUB_LOGGING)
                this.PubNubInstance.PNLog.WriteToLog  (string.Format("Integer Value {0}-{1}", key, kvp.Value), PNLoggingMethod.LevelInfo);
#endif
                FillGrantToken(parent, key, kvp.Value, typeof(int), ref pnGrantTokenDecoded);
            }
#if (ENABLE_PUBNUB_LOGGING)
            else {
                this.PubNubInstance.PNLog.WriteToLog  (string.Format("Others Key Value {0}-{1}-{2}-{3}", kvp.Key.Type, kvp.Value.Type, key, kvp.Value), PNLoggingMethod.LevelError);                
            }
#endif
        }

        public string ReplaceBoundaryQuotes(string key)
        {
            if (key != null && !string.IsNullOrEmpty(key) && key.Length >= 2 
                && key.Substring(0, 1).CompareTo("\"") == 0
                && key.Substring(key.Length - 1, 1).CompareTo("\"") == 0)
            {
                key = key.Remove(key.Length - 1, 1).Remove(0, 1);
            }
            return key;
        }

        public void FillGrantToken(string parent, string key, object val, Type type, ref PNGrantToken pnGrantTokenDecoded)
        {
            key = ReplaceBoundaryQuotes(key);
            int i = 0;
            long l = 0;
            switch (type.Name)
            {
                case "Int32":
                    if (!int.TryParse(val.ToString(), out i))
                    {
                        //log
                    }
                    break;
                case "Int64":
                    if (!long.TryParse(val.ToString(), out l))
                    {
                        //log
                    }
                    break;
                case "String":
                    // do nothing 
                    break;
                default:
                    break;
            }
            switch (key)
            {
                case "v":
                    pnGrantTokenDecoded.Version = i;
                    break;
                case "t":
                    pnGrantTokenDecoded.Timestamp = i;
                    break;
                case "ttl":
                    pnGrantTokenDecoded.TTL = i;
                    break;
                case "meta":
                    pnGrantTokenDecoded.Meta = val as Dictionary<string, object>;
                    break;
                case "sig":
#if NETSTANDARD10 || NETSTANDARD11
                    UTF8Encoding utf8 = new UTF8Encoding(true, true);
                    byte[] keyBytes = (byte[])val;
                    pnGrantTokenDecoded.Signature = utf8.GetString(keyBytes, 0, keyBytes.Length);
#else
                    byte[] sigBytes = ((CBORObject)val).GetByteString();// new byte[val.ToString().Length/2];// Encoding.ASCII.GetString((byte[])val);
                    //for (var index = 0; i < val.ToString().Length; index++)
                    //{
                    //    sign_bytes[index] = Convert.ToByte(val.ToString().Substring(index * 2, 2), 16);
                    //}
                    string base64String = Convert.ToBase64String(sigBytes);
                    pnGrantTokenDecoded.Signature = base64String;
#endif
                    break;
                default:
                    PNResourcePermission rp = GetResourcePermission(i);
                    switch (parent)
                    {
                        case "res:spc":
                            pnGrantTokenDecoded.Spaces.Add(key, rp);
                            break;
                        case "res:usr":
                            pnGrantTokenDecoded.Users.Add(key, rp);
                            break;
                        case "res:chan":
                            pnGrantTokenDecoded.Channels.Add(key, rp);
                            break;
                        case "res:grp":
                            pnGrantTokenDecoded.ChannelGroups.Add(key, rp);
                            break;
                        case "pat:spc":
                            pnGrantTokenDecoded.SpacePatterns.Add(key, rp);
                            break;
                        case "pat:usr":
                            pnGrantTokenDecoded.UserPatterns.Add(key, rp);
                            break;
                        case "pat:chan":
                            pnGrantTokenDecoded.ChannelPatterns.Add(key, rp);
                            break;
                        case "pat:grp":
                            pnGrantTokenDecoded.GroupPatterns.Add(key, rp);
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        public void SetToken(string token)
        {
            PNGrantToken tokenObj = ParseToken(token);
            if (tokenObj != null)
            {
#region "Non-Pattern Resources"
                if (tokenObj.Channels != null && tokenObj.Channels.Count > 0)
                {
                    foreach(KeyValuePair<string, PNResourcePermission> kvp in tokenObj.Channels)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "channel", ResourceId = kvp.Key, PatternFlag = 0 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
                if (tokenObj.ChannelGroups != null && tokenObj.ChannelGroups.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.ChannelGroups)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "group", ResourceId = kvp.Key, PatternFlag = 0 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
                if (tokenObj.Users != null && tokenObj.Users.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.Users)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "user", ResourceId = kvp.Key, PatternFlag = 0 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
                if (tokenObj.Spaces != null && tokenObj.Spaces.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.Spaces)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "space", ResourceId = kvp.Key, PatternFlag = 0 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
#endregion
#region "Pattern Resources"
                if (tokenObj.ChannelPatterns != null && tokenObj.ChannelPatterns.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.ChannelPatterns)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "channel", ResourceId = kvp.Key, PatternFlag = 1 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
                if (tokenObj.GroupPatterns != null && tokenObj.GroupPatterns.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.GroupPatterns)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "group", ResourceId = kvp.Key, PatternFlag = 1 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
                if (tokenObj.UserPatterns != null && tokenObj.UserPatterns.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.UserPatterns)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "user", ResourceId = kvp.Key, PatternFlag = 1 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
                if (tokenObj.SpacePatterns != null && tokenObj.SpacePatterns.Count > 0)
                {
                    foreach (KeyValuePair<string, PNResourcePermission> kvp in tokenObj.SpacePatterns)
                    {
                        PNTokenKey key = new PNTokenKey { ResourceType = "space", ResourceId = kvp.Key, PatternFlag = 1 };
                        dicToken[pubnubInstanceId].AddOrUpdate(key, token, (oldVal, newVal) => token);
                    }
                }
#endregion
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(dicToken, new TokenManagerConverter()));
#endif
        }

        public string GetToken(string resourceType, string resourceId)
        {
            string resultToken = "";

            PNTokenKey key = new PNTokenKey { ResourceType = resourceType, ResourceId = resourceId, PatternFlag = 0 };
            if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken[pubnubInstanceId].ContainsKey(key))
            {
                resultToken = dicToken[pubnubInstanceId][key];
            }
            else
            {
                resultToken = GetToken(resourceType, resourceId, true);
            }

            return resultToken;
        }

        public string GetToken(string resourceType, string resourceId, bool pattern)
        {
            string resultToken = "";
            int patterFlag = (pattern) ? 1 : 0;
            try
            {
                List<string> tokenKeyPatternList = dicToken[pubnubInstanceId].Keys.Where(k => patterFlag == k.PatternFlag && resourceType == k.ResourceType && Regex.IsMatch(resourceId, k.ResourceId)).Select(k => k.ResourceId).ToList();
                string targetResourceId = (tokenKeyPatternList != null && tokenKeyPatternList.Count > 0) ? tokenKeyPatternList[0] : "";

                PNTokenKey key = new PNTokenKey { ResourceType = resourceType, ResourceId = targetResourceId, PatternFlag = patterFlag };
                if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken[pubnubInstanceId].ContainsKey(key))
                {
                    resultToken = dicToken[pubnubInstanceId][key];
                }
            }
            catch (Exception ex)
            {
                if (this.pubnubLog != null && this.pubnubConfig != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, ex.ToString(), pubnubConfig.LogVerbosity);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            return resultToken;
        }

        public Dictionary<PNTokenKey,string> GetAllTokens()
        {
            Dictionary<PNTokenKey, string> tokenList = null;

            if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken != null && dicToken.ContainsKey(pubnubInstanceId))
            {
                ConcurrentDictionary<PNTokenKey, string> currentInstanceTokens = dicToken[pubnubInstanceId];
                tokenList = new Dictionary<PNTokenKey, string>(currentInstanceTokens);
            }

            return tokenList;
        }

        public Dictionary<PNTokenKey, string> GetTokensByResource(string resourceType)
        {
            Dictionary<PNTokenKey, string> tokenList = null;

            if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken != null && dicToken.ContainsKey(pubnubInstanceId))
            {
                tokenList = dicToken[pubnubInstanceId].Where(tk=> tk.Key.ResourceType == resourceType).ToDictionary(kvp=> kvp.Key, kvp=> kvp.Value);
            }

            return tokenList;
        }

        public void ClearTokens()
        {
            if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken != null && dicToken.ContainsKey(pubnubInstanceId))
            {
                dicToken[pubnubInstanceId].Clear();
            }
        }

        private static PNResourcePermission GetResourcePermission(int combinedVal)
        {
            PNResourcePermission rp = new PNResourcePermission();
            if (combinedVal > 0)
            {
                rp.Read = (combinedVal & (int)GrantBitFlag.READ) != 0;
                rp.Write = (combinedVal & (int)GrantBitFlag.WRITE) != 0;
                rp.Manage = (combinedVal & (int)GrantBitFlag.MANAGE) != 0;
                rp.Create = (combinedVal & (int)GrantBitFlag.CREATE) != 0;
                rp.Delete = (combinedVal & (int)GrantBitFlag.DELETE) != 0;
            }
            return rp;
        }

        internal void Destroy()
        {
            if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken != null && dicToken.ContainsKey(pubnubInstanceId))
            {
                dicToken[pubnubInstanceId].Clear();
                dicToken[pubnubInstanceId] = null;
            }
            
        }

#region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                if (!string.IsNullOrEmpty(pubnubInstanceId) && dicToken != null && dicToken.ContainsKey(pubnubInstanceId))
                {
                    dicToken[pubnubInstanceId].Clear();
                    dicToken[pubnubInstanceId] = null;
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            DisposeInternal(true);
        }
#endregion

    }
}
