using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PubnubApi.CBOR;
using System.Collections;
using System.Reflection;
#if DEBUG && NET461
#endif
using Newtonsoft.Json;

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

        internal class TokenManagerConverter : JsonConverter
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

        public PNGrantToken ParseToken(string token)
        {
            PNGrantToken result = null;
            try
            {
                if (!string.IsNullOrEmpty(token) && token.Trim().Length > 0)
                {
                    string refinedToken = token.Replace('_', '/').Replace('-', '+');
                    byte[] tokenByteArray = Convert.FromBase64String(refinedToken);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(tokenByteArray);

                    object cborItemListObj = ms.DecodeAllCBORItems();
                    if (cborItemListObj != null)
                    {
                        System.Diagnostics.Debug.WriteLine(jsonLib.SerializeToJsonString(cborItemListObj));

                        List<object> cborItemList = cborItemListObj as List<object>;
                        if (cborItemList != null && cborItemList.Count > 0)
                        {
                            object tokenItem = cborItemList[0];
                            Dictionary<string, object> dicTokenContainer = jsonLib.ConvertToDictionaryObject(tokenItem);
                            if (dicTokenContainer != null)
                            {
                                result = new PNGrantToken();

                                int tokenVersion;
                                if (dicTokenContainer.ContainsKey("v"))
                                {
                                    if (Int32.TryParse(dicTokenContainer["v"].ToString(), out tokenVersion))
                                    {
                                        result.Version = tokenVersion;
                                    }
                                }

                                long tokenTs;
                                if (dicTokenContainer.ContainsKey("t"))
                                {
                                    if (Int64.TryParse(dicTokenContainer["t"].ToString(), out tokenTs))
                                    {
                                        result.Timestamp = tokenTs;
                                    }
                                }

                                int tokenTtl;
                                if (dicTokenContainer.ContainsKey("ttl"))
                                {
                                    if (Int32.TryParse(dicTokenContainer["ttl"].ToString(), out tokenTtl))
                                    {
                                        result.TTL = tokenTtl;
                                    }
                                }

                                if (dicTokenContainer.ContainsKey("res"))
                                {
                                    Dictionary<string, object> dicResPerm = jsonLib.ConvertToDictionaryObject(dicTokenContainer["res"]);
                                    if (dicResPerm != null)
                                    {
                                        if (dicResPerm.ContainsKey("chan"))
                                        {
                                            result.Channels = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicChannelPerm = jsonLib.ConvertToDictionaryObject(dicResPerm["chan"]);
                                            if (dicChannelPerm != null && dicChannelPerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicChannelPerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.Channels.Add(kvp.Key, rp);
                                                }
                                            }
                                        }
                                        if (dicResPerm.ContainsKey("grp"))
                                        {
                                            result.ChannelGroups = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicCgPerm = jsonLib.ConvertToDictionaryObject(dicResPerm["grp"]);
                                            if (dicCgPerm != null && dicCgPerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicCgPerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.ChannelGroups.Add(kvp.Key, rp);
                                                }
                                            }
                                        }
                                        if (dicResPerm.ContainsKey("usr"))
                                        {
                                            result.Users = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicUserPerm = jsonLib.ConvertToDictionaryObject(dicResPerm["usr"]);
                                            if (dicUserPerm != null && dicUserPerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicUserPerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.Users.Add(kvp.Key, rp);
                                                }
                                            }
                                        }
                                        if (dicResPerm.ContainsKey("spc"))
                                        {
                                            result.Spaces = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicSpacePerm = jsonLib.ConvertToDictionaryObject(dicResPerm["spc"]);
                                            if (dicSpacePerm != null && dicSpacePerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicSpacePerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.Spaces.Add(kvp.Key, rp);
                                                }
                                            }
                                        }

                                    }
                                }

                                if (dicTokenContainer.ContainsKey("pat"))
                                {
                                    Dictionary<string, object> dicResPerm = jsonLib.ConvertToDictionaryObject(dicTokenContainer["pat"]);
                                    if (dicResPerm != null)
                                    {
                                        if (dicResPerm.ContainsKey("chan"))
                                        {
                                            result.ChannelPatterns = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicChannelPerm = jsonLib.ConvertToDictionaryObject(dicResPerm["chan"]);
                                            if (dicChannelPerm != null && dicChannelPerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicChannelPerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.ChannelPatterns.Add(kvp.Key, rp);
                                                }
                                            }
                                        }
                                        if (dicResPerm.ContainsKey("grp"))
                                        {
                                            result.GroupPatterns = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicCgPerm = jsonLib.ConvertToDictionaryObject(dicResPerm["grp"]);
                                            if (dicCgPerm != null && dicCgPerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicCgPerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.GroupPatterns.Add(kvp.Key, rp);
                                                }
                                            }
                                        }
                                        if (dicResPerm.ContainsKey("usr"))
                                        {
                                            result.UserPatterns = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicUserPerm = jsonLib.ConvertToDictionaryObject(dicResPerm["usr"]);
                                            if (dicUserPerm != null && dicUserPerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicUserPerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.UserPatterns.Add(kvp.Key, rp);
                                                }
                                            }
                                        }
                                        if (dicResPerm.ContainsKey("spc"))
                                        {
                                            result.SpacePatterns = new Dictionary<string, PNResourcePermission>();
                                            Dictionary<string, object> dicSpacePerm = jsonLib.ConvertToDictionaryObject(dicResPerm["spc"]);
                                            if (dicSpacePerm != null && dicSpacePerm.Count > 0)
                                            {
                                                foreach (KeyValuePair<string, object> kvp in dicSpacePerm)
                                                {
                                                    int maskedPerm;
                                                    int.TryParse(kvp.Value.ToString(), out maskedPerm);
                                                    PNResourcePermission rp = GetResourcePermission(maskedPerm);
                                                    result.SpacePatterns.Add(kvp.Key, rp);
                                                }
                                            }
                                        }

                                    }
                                }

                                if (dicTokenContainer.ContainsKey("meta"))
                                {
                                    Dictionary<string, object> dicMeta = jsonLib.ConvertToDictionaryObject(dicTokenContainer["meta"]);
                                    result.Meta = dicMeta;
                                }

                                if (dicTokenContainer.ContainsKey("sig"))
                                {
                                    byte[] sigBytes = (byte[])dicTokenContainer["sig"];
                                    string base64String = Convert.ToBase64String(sigBytes);
                                    result.Signature = base64String;
                                }
                            }
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
