using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class TokenManager : IDisposable
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly string pubnubInstanceId;
        private readonly PubnubLogModule logger;

        private static ConcurrentDictionary<string, string> dToken { get; set; } =
            new ConcurrentDictionary<string, string>();

        public TokenManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, string instanceId)
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
            pubnubInstanceId = instanceId;
            logger = config.Logger;
            if (!dToken.ContainsKey(instanceId))
            {
                dToken.TryAdd(instanceId, null);
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
                    logger?.Debug($"TokenManager Token Bytes = {GetDisplayableBytes(tokenByteArray)}");
                    
                    var cborObj = CBOR.Decode(CBOR.BinaryToHex(tokenByteArray));

                    if (cborObj is Dictionary<object, object> cborDict)
                    {
                        result = new PNTokenContent();
                        result.Meta = new Dictionary<string, object>();
                        result.Resources = new PNTokenResources
                        {
                            Channels = new Dictionary<string, PNTokenAuthValues>(),
                            ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
                            Uuids = new Dictionary<string, PNTokenAuthValues>(),
                            Users = new Dictionary<string, PNTokenAuthValues>(),
                            Spaces = new Dictionary<string, PNTokenAuthValues>()
                        };
                        result.Patterns = new PNTokenPatterns
                        {
                            Channels = new Dictionary<string, PNTokenAuthValues>(),
                            ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
                            Uuids = new Dictionary<string, PNTokenAuthValues>(),
                            Users = new Dictionary<string, PNTokenAuthValues>(),
                            Spaces = new Dictionary<string, PNTokenAuthValues>()
                        };
                        ParseCBOR(cborDict, ref result);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Error($"Error parsing token: {ex.Message} \n {ex.StackTrace}");
            }
            return result;
        }

        private string ByteToString(object value)
        {
            return value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : value.ToString();
        }

        private void FillTokenPermissionMapping(Dictionary<object, object> permissionValues,
            PNTokenPermissionMappingBase permissionMapping)
        {
            foreach (var resourceKvp in permissionValues)
            {
                var resourceType = ByteToString(resourceKvp.Key);
                foreach (var authKvp in resourceKvp.Value as Dictionary<object, object>)
                {
                    var resourceId = ByteToString(authKvp.Key);
                    var authValuesAsInt = Convert.ToInt32(authKvp.Value);
                    var resourcePermission = GetResourcePermission(authValuesAsInt);
                    switch (resourceType)
                    {
                        case "spc":
                            if (!permissionMapping.Spaces.ContainsKey(resourceId))
                            {
                                permissionMapping.Spaces.Add(resourceId, resourcePermission);
                            }

                            break;
                        case "usr":
                            if (!permissionMapping.Users.ContainsKey(resourceId))
                            {
                                permissionMapping.Users.Add(resourceId, resourcePermission);
                            }

                            break;
                        case "uuid":
                            if (!permissionMapping.Uuids.ContainsKey(resourceId))
                            {
                                permissionMapping.Uuids.Add(resourceId, resourcePermission);
                            }

                            break;
                        case "chan":
                            if (!permissionMapping.Channels.ContainsKey(resourceId))
                            {
                                permissionMapping.Channels.Add(resourceId, resourcePermission);
                            }

                            break;
                        case "grp":
                            if (!permissionMapping.ChannelGroups.ContainsKey(resourceId))
                            {
                                permissionMapping.ChannelGroups.Add(resourceId, resourcePermission);
                            }
                            break;
                        default:
                            logger?.Error($"Unidentified resource ID when parsing permission mappings! {resourceId}");
                            break;
                    }
                }
            }
        }

        private void ParseCBOR(Dictionary<object, object> cbor, ref PNTokenContent pnGrantTokenDecoded)
        {
            foreach (var kvp in cbor)
            {
                var key = ByteToString(kvp.Key);
                switch (key)
                {
                    case "v":
                        pnGrantTokenDecoded.Version = Convert.ToInt32(kvp.Value);
                        break;
                    case "t":
                        pnGrantTokenDecoded.Timestamp = Convert.ToInt64(kvp.Value);
                        break;
                    case "ttl":
                        pnGrantTokenDecoded.TTL = Convert.ToInt32(kvp.Value);
                        break;
                    case "meta":
                        pnGrantTokenDecoded.Meta = (kvp.Value as Dictionary<object, object>).ToDictionary(x => ByteToString(x.Key), y => y.Value);
                        break;
                    case "uuid":
                        pnGrantTokenDecoded.AuthorizedUuid = ByteToString(kvp.Value);
                        break;
                    case "sig":
                        byte[] sigBytes = kvp.Value as byte[];
                        string base64String = Convert.ToBase64String(sigBytes);
                        pnGrantTokenDecoded.Signature = base64String;
                        break;
                    case "res":
                        FillTokenPermissionMapping(kvp.Value as Dictionary<object, object>, pnGrantTokenDecoded.Resources);
                        break;
                    case "pat":
                        FillTokenPermissionMapping(kvp.Value as Dictionary<object, object>, pnGrantTokenDecoded.Patterns);
                        break;
                    default:
                        logger?.Error($"Unidentified key when parsing token! {key}");
                        break;
                }
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