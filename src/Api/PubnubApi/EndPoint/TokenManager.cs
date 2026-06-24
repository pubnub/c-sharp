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
                            Spaces = new Dictionary<string, PNTokenAuthValues>(),
                            DataSync = NewDataSyncScopes()
                        };
                        result.Patterns = new PNTokenPatterns
                        {
                            Channels = new Dictionary<string, PNTokenAuthValues>(),
                            ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
                            Uuids = new Dictionary<string, PNTokenAuthValues>(),
                            Users = new Dictionary<string, PNTokenAuthValues>(),
                            Spaces = new Dictionary<string, PNTokenAuthValues>(),
                            DataSync = NewDataSyncScopes()
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
                        case "datasync:entities":
                            if (!permissionMapping.DataSync.Entities.ContainsKey(resourceId))
                            {
                                permissionMapping.DataSync.Entities.Add(resourceId, resourcePermission);
                            }
                            break;
                        case "datasync:relationships":
                            if (!permissionMapping.DataSync.Relationships.ContainsKey(resourceId))
                            {
                                permissionMapping.DataSync.Relationships.Add(resourceId, resourcePermission);
                            }
                            break;
                        case "datasync:memberships":
                            if (!permissionMapping.DataSync.Memberships.ContainsKey(resourceId))
                            {
                                permissionMapping.DataSync.Memberships.Add(resourceId, resourcePermission);
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
                        if (pnGrantTokenDecoded.Meta.TryGetValue("pn-projections", out var projectionsValue))
                        {
                            pnGrantTokenDecoded.Projections = DecodeProjections(projectionsValue as Dictionary<object, object>);
                        }
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

        private static PNDataSyncTokenScopes NewDataSyncScopes()
        {
            return new PNDataSyncTokenScopes
            {
                Entities = new Dictionary<string, PNTokenAuthValues>(),
                Relationships = new Dictionary<string, PNTokenAuthValues>(),
                Memberships = new Dictionary<string, PNTokenAuthValues>()
            };
        }

        private PNDataSyncProjections DecodeProjections(Dictionary<object, object> projections)
        {
            if (projections == null)
            {
                return null;
            }

            var result = new PNDataSyncProjections();
            foreach (var kvp in projections)
            {
                var scopeKey = ByteToString(kvp.Key);
                var scope = DecodeProjectionScope(kvp.Value as Dictionary<object, object>);
                switch (scopeKey)
                {
                    case "res":
                        result.Resources = scope;
                        break;
                    case "pat":
                        result.Patterns = scope;
                        break;
                    default:
                        logger?.Error($"Unidentified projection scope key when parsing token! {scopeKey}");
                        break;
                }
            }
            return result;
        }

        private PNDataSyncProjectionScope DecodeProjectionScope(Dictionary<object, object> scopeValues)
        {
            var scope = new PNDataSyncProjectionScope
            {
                Entities = new Dictionary<string, string>(),
                Relationships = new Dictionary<string, string>(),
                Memberships = new Dictionary<string, string>()
            };
            if (scopeValues == null)
            {
                return scope;
            }

            foreach (var kvp in scopeValues)
            {
                // Flat composite key, e.g. "datasync:entities:user.A" -> projection name.
                var compositeKey = ByteToString(kvp.Key);
                var projectionName = ByteToString(kvp.Value);

                const string prefix = "datasync:";
                if (!compositeKey.StartsWith(prefix, StringComparison.Ordinal))
                {
                    logger?.Error($"Unidentified projection key when parsing token! {compositeKey}");
                    continue;
                }

                var remainder = compositeKey.Substring(prefix.Length);
                var separatorIndex = remainder.IndexOf(':');
                if (separatorIndex <= 0 || separatorIndex >= remainder.Length - 1)
                {
                    logger?.Error($"Malformed projection key when parsing token! {compositeKey}");
                    continue;
                }

                var type = remainder.Substring(0, separatorIndex);
                var resourceId = remainder.Substring(separatorIndex + 1);
                switch (type)
                {
                    case "entities":
                        scope.Entities[resourceId] = projectionName;
                        break;
                    case "relationships":
                        scope.Relationships[resourceId] = projectionName;
                        break;
                    case "memberships":
                        scope.Memberships[resourceId] = projectionName;
                        break;
                    default:
                        logger?.Error($"Unidentified projection resource type when parsing token! {type}");
                        break;
                }
            }
            return scope;
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