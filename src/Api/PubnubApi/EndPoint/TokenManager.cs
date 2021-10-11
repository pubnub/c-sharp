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
using System.Globalization;

namespace PubnubApi.EndPoint
{
    public class TokenManager : IDisposable
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;
        private readonly string pubnubInstanceId;
        private static ConcurrentDictionary<string, string> dToken
        {
            get;
            set;
        } = new ConcurrentDictionary<string, string>();

        public TokenManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log, string instanceId)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubInstanceId = instanceId;
            if (!dToken.ContainsKey(instanceId))
            {
                dToken.GetOrAdd(instanceId, null);
            }
            
        }

        public string AuthToken 
        {
            get 
            { 
                string tkn;
                dToken.TryGetValue(pubnubInstanceId, out tkn); 
                return tkn; 
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

        public PNTokenContent ParseToken(string token)
        {
            PNTokenContent result = null;
            try
            {
                if (!string.IsNullOrEmpty(token) && token.Trim().Length > 0)
                {
                    string refinedToken = token.Replace('_', '/').Replace('-', '+');
                    byte[] tokenByteArray = Convert.FromBase64String(refinedToken);
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Token Bytes = {1}", DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture), GetDisplayableBytes(tokenByteArray)), PNLogVerbosity.BODY);
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(tokenByteArray))
                    {
                        CBORObject cborObj = CBORObject.DecodeFromBytes(tokenByteArray);
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, RAW CBOR {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), cborObj.ToJSONString()), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);

                        if (cborObj != null)
                        {
                            result = new PNTokenContent();
                            result.Meta = new Dictionary<string, object>();
                            result.Resources = new PNTokenResources() { 
                                Channels = new Dictionary<string, PNTokenAuthValues>(),
                                ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
                                Uuids = new Dictionary<string, PNTokenAuthValues>(),
                                Users = new Dictionary<string, PNTokenAuthValues>(),
                                Spaces = new Dictionary<string, PNTokenAuthValues>()
                            };
                            result.Patterns = new PNTokenPatterns() {
                                Channels = new Dictionary<string, PNTokenAuthValues>(),
                                ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
                                Uuids = new Dictionary<string, PNTokenAuthValues>(),
                                Users = new Dictionary<string, PNTokenAuthValues>(),
                                Spaces = new Dictionary<string, PNTokenAuthValues>()
                            };
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

        private void ParseCBOR(CBORObject cbor, string parent, ref PNTokenContent pnGrantTokenDecoded)
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
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TextString Key {1}-{2}-{3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), kvp.Key.ToString(), kvp.Value.ToString(), kvp.Value.Type), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
                    ParseCBORValue(kvp.Key.ToString(), parent, kvp, ref pnGrantTokenDecoded);
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Others Key {1}-{2}-{3}-{4}", DateTime.Now.ToString(CultureInfo.InvariantCulture), kvp.Key, kvp.Key.Type, kvp.Value, kvp.Value.Type), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
                }

            }
        }

        private void ParseCBORValue(string key, string parent, KeyValuePair<CBORObject, CBORObject> kvp, ref PNTokenContent pnGrantTokenDecoded)
        {
            if (kvp.Value.Type.ToString().Equals("Map"))
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Map Key {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
                var p = string.Format("{0}{1}{2}", parent, string.IsNullOrEmpty(parent) ? "" : ":", key);
                ParseCBOR(kvp.Value, p, ref pnGrantTokenDecoded);
            }
            else if (kvp.Value.Type.ToString().Equals("ByteString"))
            {
#if NETSTANDARD10 || NETSTANDARD11
                    UTF8Encoding utf8 = new UTF8Encoding(true, true);
                    byte[] valBytes = kvp.Value.GetByteString();
                    string val = utf8.GetString(valBytes, 0, valBytes.Length);
#else
                string val = System.Text.Encoding.ASCII.GetString(kvp.Value.GetByteString());
#endif
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, ByteString Value {1}-{2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key, val), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
                FillGrantToken(parent, key, kvp.Value, typeof(string), ref pnGrantTokenDecoded);
            }
            else if (kvp.Value.Type.ToString().Equals("Integer"))
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Integer Value {1}-{2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key, kvp.Value), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
                FillGrantToken(parent, key, kvp.Value, typeof(int), ref pnGrantTokenDecoded);
            }
            else if (kvp.Value.Type.ToString().Equals("TextString"))
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TextString Value {1}-{2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), key, kvp.Value), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
                FillGrantToken(parent, key, kvp.Value, typeof(string), ref pnGrantTokenDecoded);
            }
            else
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Others Key Value {1}-{2}-{3}-{4}", DateTime.Now.ToString(CultureInfo.InvariantCulture), kvp.Key.Type, kvp.Value.Type, key, kvp.Value), (pubnubConfig != null) ? pubnubConfig.LogVerbosity : PNLogVerbosity.NONE);
            }
        }

        private string ReplaceBoundaryQuotes(string key)
        {
            if (key != null && !string.IsNullOrEmpty(key) && key.Length >= 2 
                && key.Substring(0, 1).CompareTo("\"") == 0
                && key.Substring(key.Length - 1, 1).CompareTo("\"") == 0)
            {
                key = key.Remove(key.Length - 1, 1).Remove(0, 1);
            }
            return key;
        }

        private void FillGrantToken(string parent, string key, object val, Type type, ref PNTokenContent pnGrantTokenDecoded)
        {
            key = ReplaceBoundaryQuotes(key);
            int i = 0;
            long l = 0;
            switch (type.Name)
            {
                case "Int32":
                    if (!int.TryParse(val.ToString(), out i))
                    {
                        //do nothing.
                    }
                    break;
                case "Int64":
                    if (!long.TryParse(val.ToString(), out l))
                    {
                        //do nothing
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
                case "uuid":
                    pnGrantTokenDecoded.AuthorizedUuid = ((CBORObject)val).AsString();
                    break;
                case "sig":
#if NETSTANDARD10 || NETSTANDARD11
                    UTF8Encoding utf8 = new UTF8Encoding(true, true);
                    byte[] keyBytes = (byte[])val;
                    pnGrantTokenDecoded.Signature = utf8.GetString(keyBytes, 0, keyBytes.Length);
#else
                    byte[] sigBytes = ((CBORObject)val).GetByteString();
                    string base64String = Convert.ToBase64String(sigBytes);
                    pnGrantTokenDecoded.Signature = base64String;
#endif
                    break;
                default:
                    PNTokenAuthValues rp = GetResourcePermission(i);
                    switch (parent)
                    {
                        case "meta":
                            if (!pnGrantTokenDecoded.Meta.ContainsKey(key))
                            {
                                switch (type.Name)
                                {
                                    case "Int32":
                                        pnGrantTokenDecoded.Meta.Add(key, ((CBORObject)val).AsInt32());
                                        break;
                                    case "Int64":
                                        pnGrantTokenDecoded.Meta.Add(key, ((CBORObject)val).ToObject<long>());
                                        break;
                                    case "String":
                                        pnGrantTokenDecoded.Meta.Add(key, ((CBORObject)val).AsString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "res:spc":
                            if (!pnGrantTokenDecoded.Resources.Spaces.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Resources.Spaces.Add(key, rp);
                            }
                            break;
                        case "res:usr":
                            if (!pnGrantTokenDecoded.Resources.Users.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Resources.Users.Add(key, rp);
                            }
                            break;
                        case "res:uuid":
                            if (!pnGrantTokenDecoded.Resources.Uuids.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Resources.Uuids.Add(key, rp);
                            }
                            break;
                        case "res:chan":
                            if (!pnGrantTokenDecoded.Resources.Channels.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Resources.Channels.Add(key, rp);
                            }
                            break;
                        case "res:grp":
                            if (!pnGrantTokenDecoded.Resources.ChannelGroups.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Resources.ChannelGroups.Add(key, rp);
                            }
                            break;
                        case "pat:spc":
                            if (!pnGrantTokenDecoded.Patterns.Spaces.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Patterns.Spaces.Add(key, rp);
                            }
                            break;
                        case "pat:usr":
                            if (!pnGrantTokenDecoded.Patterns.Users.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Patterns.Users.Add(key, rp);
                            }
                            break;
                        case "pat:uuid":
                            if (!pnGrantTokenDecoded.Patterns.Uuids.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Patterns.Uuids.Add(key, rp);
                            }
                            break;
                        case "pat:chan":
                            if (!pnGrantTokenDecoded.Patterns.Channels.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Patterns.Channels.Add(key, rp);
                            }
                            break;
                        case "pat:grp":
                            if (!pnGrantTokenDecoded.Patterns.ChannelGroups.ContainsKey(key))
                            {
                                pnGrantTokenDecoded.Patterns.ChannelGroups.Add(key, rp);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        public void SetAuthToken(string token)
        {
            dToken.AddOrUpdate(pubnubInstanceId, token, (k, o) => token);
        }

        private static PNTokenAuthValues GetResourcePermission(int combinedVal)
        {
            PNTokenAuthValues rp = new PNTokenAuthValues();
            if (combinedVal > 0)
            {
                rp.Read = (combinedVal & (int)GrantBitFlag.READ) != 0;
                rp.Write = (combinedVal & (int)GrantBitFlag.WRITE) != 0;
                rp.Manage = (combinedVal & (int)GrantBitFlag.MANAGE) != 0;
                rp.Create = (combinedVal & (int)GrantBitFlag.CREATE) != 0;
                rp.Delete = (combinedVal & (int)GrantBitFlag.DELETE) != 0;
                rp.Get = (combinedVal & (int)GrantBitFlag.GET) != 0;
                rp.Update = (combinedVal & (int)GrantBitFlag.UPDATE) != 0;
                rp.Join = (combinedVal & (int)GrantBitFlag.JOIN) != 0;
            }
            return rp;
        }

        internal void Destroy()
        {
            if (!string.IsNullOrEmpty(pubnubInstanceId) && dToken != null && dToken.ContainsKey(pubnubInstanceId))
            {
                dToken[pubnubInstanceId] = null;
            }
            
        }

#region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                if (!string.IsNullOrEmpty(pubnubInstanceId) && dToken != null && dToken.ContainsKey(pubnubInstanceId))
                {
                    dToken[pubnubInstanceId] = null;
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
