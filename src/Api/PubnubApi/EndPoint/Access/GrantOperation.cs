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
    public class GrantOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private Dictionary<string,PNResourcePermission> pubnubChannelNames = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubChannelGroupNames = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubUsers = new Dictionary<string, PNResourcePermission>();
        private Dictionary<string, PNResourcePermission> pubnubSpaces = new Dictionary<string, PNResourcePermission>();
        private string[] pamAuthenticationKeys;
        private bool grantWrite;
        private bool grantRead;
        private bool grantManage;
        private bool grantDelete;
        private bool grantCreate;
        private long grantTTL = -1;
        private PNCallback<PNAccessManagerGrantResult> savedCallbackGrantResult;
        private PNCallback<PNAccessManagerTokenResult> savedCallbackGrantToken;
        private Dictionary<string, object> queryParam;
        private Dictionary<string, object> grantMeta;

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public GrantOperation Channels(string[] channels)
        {
            Dictionary<string, PNResourcePermission> chDic = chDic = new Dictionary<string, PNResourcePermission>();
            if (channels != null)
            {
                for (int index=0; index < channels.Length; index++)
                {
                    chDic.Add(channels[index], null);
                }
            }
            this.pubnubChannelNames = chDic;
            return this;
        }

        public GrantOperation Channels(Dictionary<string, PNResourcePermission> channelPermissions)
        {
            if (channelPermissions != null)
            {
                this.pubnubChannelNames = channelPermissions;
            }
            return this;
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public GrantOperation ChannelGroups(string[] channelGroups)
        {
            Dictionary<string, PNResourcePermission> cgDic = cgDic = new Dictionary<string, PNResourcePermission>();
            if (channelGroups != null && channelGroups.Length > 0)
            {
                for (int index = 0; index < channelGroups.Length; index++)
                {
                    cgDic.Add(channelGroups[index], null);
                }
            }
            this.pubnubChannelGroupNames = cgDic;
            return this;
        }

        public GrantOperation ChannelGroups(Dictionary<string, PNResourcePermission> channelGroupPermissions)
        {
            if (channelGroupPermissions != null)
            {
                this.pubnubChannelNames = channelGroupPermissions;
            }
            return this;
        }

        public GrantOperation Users(Dictionary<string, PNResourcePermission> userPermissions)
        {
            if (userPermissions != null)
            {
                this.pubnubUsers = userPermissions;
            }
            return this;
        }

        public GrantOperation Spaces(Dictionary<string, PNResourcePermission> spacePermissions)
        {
            if (spacePermissions != null)
            {
                this.pubnubSpaces = spacePermissions;
            }
            return this;
        }

        public GrantOperation AuthKeys(string[] authKeys)
        {
            this.pamAuthenticationKeys = authKeys;
            return this;
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public GrantOperation Write(bool write)
        {
            this.grantWrite = write;
            return this;
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public GrantOperation Read(bool read)
        {
            this.grantRead = read;
            return this;
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public GrantOperation Manage(bool manage)
        {
            this.grantManage = manage;
            return this;
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public GrantOperation Delete(bool delete)
        {
            this.grantDelete = delete;
            return this;
        }

        public GrantOperation TTL(long ttl)
        {
            this.grantTTL = ttl;
            return this;
        }

        public GrantOperation Meta(Dictionary<string, object> metaObject)
        {
            this.grantMeta = metaObject;
            return this;
        }

        public GrantOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNAccessManagerGrantResult> callback)
        {
            Execute(callback);
        }

        [Obsolete("Use GrantV2() for backward compatibility")]
        public void Execute(PNCallback<PNAccessManagerGrantResult> callback)
        {
            if (config != null && pubnubLog != null)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, WARNING: Grant() signature has changed! This specific call will be making a request to PAMv2. Please update your code if this is not the intended action.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallbackGrantResult = callback;
                this.savedCallbackGrantToken = null;
                GrantV2Access(this.pubnubChannelNames.Keys.ToArray(), this.pubnubChannelGroupNames.Keys.ToArray(), this.pamAuthenticationKeys, this.grantRead, this.grantWrite, this.grantDelete, this.grantManage, this.grantTTL, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallbackGrantResult = callback;
                this.savedCallbackGrantToken = null;
                GrantV2Access(this.pubnubChannelNames.Keys.ToArray(), this.pubnubChannelGroupNames.Keys.ToArray(), this.pamAuthenticationKeys, this.grantRead, this.grantWrite, this.grantDelete, this.grantManage, this.grantTTL, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public void Execute(PNCallback<PNAccessManagerTokenResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallbackGrantToken = callback;
                this.savedCallbackGrantResult = null;
                GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pamAuthenticationKeys, this.grantTTL, this.grantMeta, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallbackGrantToken = callback;
                this.savedCallbackGrantResult = null;
                GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pamAuthenticationKeys, this.grantTTL, this.grantMeta, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                if (savedCallbackGrantResult != null)
                {
                    GrantV2Access(this.pubnubChannelNames.Keys.ToArray(), this.pubnubChannelGroupNames.Keys.ToArray(), this.pamAuthenticationKeys, this.grantRead, this.grantWrite, this.grantDelete, this.grantManage, this.grantTTL, this.queryParam, savedCallbackGrantResult);
                }
                else if (savedCallbackGrantToken != null)
                {
                    GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pamAuthenticationKeys, this.grantTTL, this.grantMeta, this.queryParam, savedCallbackGrantToken);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                if (savedCallbackGrantResult != null)
                {
                    GrantV2Access(this.pubnubChannelNames.Keys.ToArray(), this.pubnubChannelGroupNames.Keys.ToArray(), this.pamAuthenticationKeys, this.grantRead, this.grantWrite, this.grantDelete, this.grantManage, this.grantTTL, this.queryParam, savedCallbackGrantResult);
                }
                else if (savedCallbackGrantToken != null)
                {
                    GrantAccess(this.pubnubChannelNames, this.pubnubChannelGroupNames, this.pubnubUsers, this.pubnubSpaces, this.pamAuthenticationKeys, this.grantTTL, this.grantMeta, this.queryParam, savedCallbackGrantToken);
                }
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void GrantAccess(Dictionary<string, PNResourcePermission> channelsPermission, Dictionary<string, PNResourcePermission> channelGroupsPermission, Dictionary<string, PNResourcePermission> usersPermission, Dictionary<string, PNResourcePermission> spacesPermission, string[] authKeys, long ttl, Dictionary<string, object> meta, Dictionary<string, object> externalQueryParam, PNCallback<PNAccessManagerTokenResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            RequestState<PNAccessManagerTokenResult> requestState = new RequestState<PNAccessManagerTokenResult>();
            requestState.Channels = channelsPermission.Keys.ToArray();
            requestState.ChannelGroups = channelGroupsPermission.Keys.ToArray();
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
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

            Dictionary<string, object> resourcesDic = new Dictionary<string, object>();
            resourcesDic.Add("channels", chBitmaskPermDic);
            resourcesDic.Add("groups", cgBitmaskPermDic);
            resourcesDic.Add("users", userBitmaskPermDic);
            resourcesDic.Add("spaces", spaceBitmaskPermDic);


            Dictionary<string, int> dummyBitmaskPermDic = new Dictionary<string, int>();

            Dictionary<string, object> patternsDic = new Dictionary<string, object>();
            patternsDic.Add("channels", dummyBitmaskPermDic);
            patternsDic.Add("groups", dummyBitmaskPermDic);
            patternsDic.Add("users", dummyBitmaskPermDic);
            patternsDic.Add("spaces", dummyBitmaskPermDic);

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

        internal void GrantV2Access(string[] channels, string[] channelGroups, string[] authKeys, bool read, bool write, bool delete, bool manage, long ttl, Dictionary<string, object> externalQueryParam, PNCallback<PNAccessManagerGrantResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            List<string> authList = new List<string>();

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (authKeys != null && authKeys.Length > 0)
            {
                authList = new List<string>(authKeys);
                authList = authList.Where(auth => !string.IsNullOrEmpty(auth) && auth.Trim().Length > 0).Distinct<string>().ToList();
            }

            string channelsCommaDelimited = string.Join(",", channelList.ToArray().OrderBy(x => x).ToArray());
            string channelGroupsCommaDelimited = string.Join(",", channelGroupList.ToArray().OrderBy(x => x).ToArray());
            string authKeysCommaDelimited = string.Join(",", authList.ToArray().OrderBy(x => x).ToArray());

            RequestState<PNAccessManagerGrantResult> requestState = new RequestState<PNAccessManagerGrantResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";

            Uri request = urlBuilder.BuildGrantV2AccessRequest("GET", "", channelsCommaDelimited, channelGroupsCommaDelimited, authKeysCommaDelimited, read, write, delete, manage, ttl, externalQueryParam);
            string json = UrlProcessRequest<PNAccessManagerGrantResult>(request, requestState, false);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNAccessManagerGrantResult>(requestState, json);
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
