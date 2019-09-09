using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Globalization;

namespace PubnubApi.EndPoint
{
    public class GrantTokenOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private Dictionary<string, PNResourcePermission> pubnubChannelNames = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubChannelGroupNames = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubUsers = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubSpaces = new Dictionary<string, PNResourcePermission>();

        private Dictionary<string, PNResourcePermission> pubnubChannelNamesPattern = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubChannelGroupNamesPattern = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubUsersPattern = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubSpacesPattern = new Dictionary<string, PNResourcePermission>();
        private long grantTTL = -1;
        private PNCallback<PNAccessManagerTokenResult> savedCallbackGrantToken;
        private Dictionary<string, object> queryParam;
        private Dictionary<string, object> grantMeta;

        public GrantTokenOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public GrantTokenOperation Channels(Dictionary<string, PNResourcePermission> channelPermissions)
        {
            if (channelPermissions != null)
            {
                this.pubnubChannelNames = channelPermissions;
            }
            return this;
        }

        public GrantTokenOperation Channels(Dictionary<string, PNResourcePermission> channelPermissions, bool pattern)
        {
            if (channelPermissions != null)
            {
                if (pattern)
                {
                    this.pubnubChannelNamesPattern = channelPermissions;
                }
                else
                {
                    this.pubnubChannelNames = channelPermissions;
                }
            }
            return this;
        }

        public GrantTokenOperation ChannelGroups(Dictionary<string, PNResourcePermission> channelGroupPermissions)
        {
            if (channelGroupPermissions != null)
            {
                this.pubnubChannelGroupNames = channelGroupPermissions;
            }
            return this;
        }

        public GrantTokenOperation ChannelGroups(Dictionary<string, PNResourcePermission> channelGroupPermissions, bool pattern)
        {
            if (channelGroupPermissions != null)
            {
                if (pattern)
                {
                    this.pubnubChannelGroupNamesPattern = channelGroupPermissions;
                }
                else
                {
                    this.pubnubChannelGroupNames = channelGroupPermissions;
                }
            }
            return this;
        }

        public GrantTokenOperation Users(Dictionary<string, PNResourcePermission> userPermissions)
        {
            if (userPermissions != null)
            {
                this.pubnubUsers = userPermissions;
            }
            return this;
        }

        public GrantTokenOperation Users(Dictionary<string, PNResourcePermission> userPermissions, bool pattern)
        {
            if (userPermissions != null)
            {
                if (pattern)
                {
                    this.pubnubUsersPattern = userPermissions;
                }
                else
                {
                    this.pubnubUsers = userPermissions;
                }
            }
            return this;
        }

        public GrantTokenOperation Spaces(Dictionary<string, PNResourcePermission> spacePermissions)
        {
            if (spacePermissions != null)
            {
                this.pubnubSpaces = spacePermissions;
            }
            return this;
        }

        public GrantTokenOperation Spaces(Dictionary<string, PNResourcePermission> spacePermissions, bool pattern)
        {
            if (spacePermissions != null)
            {
                if (pattern)
                {
                    this.pubnubSpacesPattern = spacePermissions;
                }
                else
                {
                    this.pubnubSpaces = spacePermissions;
                }
            }
            return this;
        }

        public GrantTokenOperation TTL(long ttl)
        {
            this.grantTTL = ttl;
            return this;
        }

        public GrantTokenOperation Meta(Dictionary<string, object> metaObject)
        {
            this.grantMeta = metaObject;
            return this;
        }

        public GrantTokenOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNAccessManagerTokenResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallbackGrantToken = callback;
                GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pubnubChannelNamesPattern, this.pubnubChannelGroupNamesPattern, this.pubnubUsersPattern, this.pubnubSpacesPattern, this.grantTTL, this.grantMeta, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallbackGrantToken = callback;
                GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pubnubChannelNamesPattern, this.pubnubChannelGroupNamesPattern, this.pubnubUsersPattern, this.pubnubSpacesPattern, this.grantTTL, this.grantMeta, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                    GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pubnubChannelNamesPattern, this.pubnubChannelGroupNamesPattern, this.pubnubUsersPattern, this.pubnubSpacesPattern, this.grantTTL, this.grantMeta, this.queryParam, savedCallbackGrantToken);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pubnubChannelNamesPattern, this.pubnubChannelGroupNamesPattern, this.pubnubUsersPattern, this.pubnubSpacesPattern, this.grantTTL, this.grantMeta, this.queryParam, savedCallbackGrantToken);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void GrantAccess(Dictionary<string, PNResourcePermission> channelsPermission, Dictionary<string, PNResourcePermission> channelGroupsPermission, Dictionary<string, PNResourcePermission> usersPermission, Dictionary<string, PNResourcePermission> spacesPermission, Dictionary<string, PNResourcePermission> channelsPatternPermission, Dictionary<string, PNResourcePermission> channelGroupsPatternPermission, Dictionary<string, PNResourcePermission> usersPatternPermission, Dictionary<string, PNResourcePermission> spacesPatternPermission, long ttl, Dictionary<string, object> meta, Dictionary<string, object> externalQueryParam, PNCallback<PNAccessManagerTokenResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            RequestState<PNAccessManagerTokenResult> requestState = new RequestState<PNAccessManagerTokenResult>();
            requestState.Channels = channelsPermission.Keys.ToArray();
            requestState.ChannelGroups = channelGroupsPermission.Keys.ToArray();
            requestState.ResponseType = PNOperationType.PNAccessManagerGrantToken;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;


            requestState.UsePostMethod = true;

            Dictionary<string, int> chBitmaskPermDic = new Dictionary<string, int>();
            foreach(KeyValuePair<string, PNResourcePermission> kvp in channelsPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                chBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }
            Dictionary<string, int> chPatternBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in channelsPatternPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                chPatternBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }


            Dictionary<string, int> cgBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in channelGroupsPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                cgBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }
            Dictionary<string, int> cgPatternBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in channelGroupsPatternPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                cgPatternBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }

            Dictionary<string, int> userBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in usersPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                userBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }
            Dictionary<string, int> userPatternBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in usersPatternPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                userPatternBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }

            Dictionary<string, int> spaceBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in spacesPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                spaceBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }
            Dictionary<string, int> spacePatternBitmaskPermDic = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PNResourcePermission> kvp in spacesPatternPermission)
            {
                PNResourcePermission perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm.Read, perm.Write, perm.Manage, perm.Delete, perm.Create);
                }
                spacePatternBitmaskPermDic.Add(kvp.Key, bitMaskPermissionValue);
            }

            Dictionary<string, object> resourcesDic = new Dictionary<string, object>();
            resourcesDic.Add("channels", chBitmaskPermDic);
            resourcesDic.Add("groups", cgBitmaskPermDic);
            resourcesDic.Add("users", userBitmaskPermDic);
            resourcesDic.Add("spaces", spaceBitmaskPermDic);

            Dictionary<string, object> patternsDic = new Dictionary<string, object>();
            patternsDic.Add("channels", chPatternBitmaskPermDic);
            patternsDic.Add("groups", cgPatternBitmaskPermDic);
            patternsDic.Add("users", userPatternBitmaskPermDic);
            patternsDic.Add("spaces", spacePatternBitmaskPermDic);

            Dictionary<string, object> optimizedMeta = new Dictionary<string, object>();
            if (meta != null)
            {
                optimizedMeta = meta;
            }

            Dictionary<string, object> permissionDic = new Dictionary<string, object>();
            permissionDic.Add("resources", resourcesDic);
            permissionDic.Add("patterns", patternsDic);
            permissionDic.Add("meta", optimizedMeta);

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("ttl", ttl);
            messageEnvelope.Add("permissions", permissionDic);
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildGrantV3AccessRequest("POST", postMessage, externalQueryParam);

            string json = UrlProcessRequest<PNAccessManagerTokenResult>(request, requestState, false, postMessage);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNAccessManagerTokenResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
        }

        private int CalculateGrantBitMaskValue(bool read, bool write, bool manage, bool delete, bool create)
        {
            int result = 0;

            if (read)
            {
                result = (int)GrantBitFlag.READ;
            }
            if (write)
            {
                result = result + (int)GrantBitFlag.WRITE;
            }
            if (manage)
            {
                result = result + (int)GrantBitFlag.MANAGE;
            }
            if (delete)
            {
                result = result + (int)GrantBitFlag.DELETE;
            }
            if (create)
            {
                result = result + (int)GrantBitFlag.CREATE;
            }

            return result;
        }
    }
}
