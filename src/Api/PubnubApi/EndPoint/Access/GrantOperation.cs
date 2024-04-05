using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class GrantOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string[] pubnubChannelNames;
        private string[] pubnubChannelGroupNames;
        private string[] pubnubTargetUuids;
        private string[] pamAuthenticationKeys;
        private bool grantWrite;
        private bool grantRead;
        private bool grantManage;
        private bool grantDelete;
        private bool grantGet;
        private bool grantUpdate;
        private bool grantJoin;
        private long grantTTL = -1;
        private PNCallback<PNAccessManagerGrantResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, null, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public GrantOperation Channels(string[] channels)
        {
            this.pubnubChannelNames = channels;
            return this;
        }

        public GrantOperation ChannelGroups(string[] channelGroups)
        {
            this.pubnubChannelGroupNames = channelGroups;
            return this;
        }

        public GrantOperation Uuids(string[] targetUuids)
        {
            this.pubnubTargetUuids = targetUuids;
            return this;
        }

        public GrantOperation AuthKeys(string[] authKeys)
        {
            this.pamAuthenticationKeys = authKeys;
            return this;
        }

        public GrantOperation Write(bool write)
        {
            this.grantWrite = write;
            return this;
        }

        public GrantOperation Read(bool read)
        {
            this.grantRead = read;
            return this;
        }

        public GrantOperation Manage(bool manage)
        {
            this.grantManage = manage;
            return this;
        }

        public GrantOperation Delete(bool delete)
        {
            this.grantDelete = delete;
            return this;
        }

        public GrantOperation Get(bool get)
        {
            this.grantGet = get;
            return this;
        }

        public GrantOperation Update(bool update)
        {
            this.grantUpdate = update;
            return this;
        }

        public GrantOperation Join(bool join)
        {
            this.grantJoin = join;
            return this;
        }

        public GrantOperation TTL(long ttl)
        {
            this.grantTTL = ttl;
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

        public void Execute(PNCallback<PNAccessManagerGrantResult> callback)
        {
            if ((this.pubnubChannelNames != null || this.pubnubChannelGroupNames != null) && this.pubnubTargetUuids != null)
            {
                throw new InvalidOperationException("Both channel/channelgroup and uuid cannot be used in the same request");
            }
            if (this.pubnubTargetUuids != null && (this.grantRead || this.grantWrite || this.grantManage || this.grantJoin))
            {
                throw new InvalidOperationException("Only Get/Update/Delete permissions are allowed for UUID");
            }
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GrantAccess(callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                GrantAccess(callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNAccessManagerGrantResult>> ExecuteAsync()
        {
            if ((this.pubnubChannelNames != null || this.pubnubChannelGroupNames != null) && this.pubnubTargetUuids != null)
            {
                throw new InvalidOperationException("Both channel/channelgroup and uuid cannot be used in the same request");
            }
            if (this.pubnubTargetUuids != null && (this.grantRead || this.grantWrite || this.grantManage || this.grantJoin))
            {
                throw new InvalidOperationException("Only Get/Update/Delete permissions are allowed for UUID");
            }
            return await GrantAccess().ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GrantAccess(savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GrantAccess(savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void GrantAccess(PNCallback<PNAccessManagerGrantResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            List<string> uuidList = new List<string>();
            List<string> authList = new List<string>();

            if (this.pubnubChannelNames != null && this.pubnubChannelNames.Length > 0)
            {
                channelList = new List<string>(this.pubnubChannelNames);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (this.pubnubChannelGroupNames != null && this.pubnubChannelGroupNames.Length > 0)
            {
                channelGroupList = new List<string>(this.pubnubChannelGroupNames);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (this.pubnubTargetUuids != null && this.pubnubTargetUuids.Length > 0)
            {
                uuidList = new List<string>(this.pubnubTargetUuids);
                uuidList = uuidList.Where(uuid => !string.IsNullOrEmpty(uuid) && uuid.Trim().Length > 0).Distinct().ToList();
            }

            if (this.pamAuthenticationKeys != null && this.pamAuthenticationKeys.Length > 0)
            {
                authList = new List<string>(this.pamAuthenticationKeys);
                authList = authList.Where(auth => !string.IsNullOrEmpty(auth) && auth.Trim().Length > 0).Distinct<string>().ToList();
            }

            string channelsCommaDelimited = string.Join(",", channelList.OrderBy(x => x).ToArray());
            string channelGroupsCommaDelimited = string.Join(",", channelGroupList.OrderBy(x => x).ToArray());
            string targetUuidsCommaDelimited = string.Join(",", uuidList.OrderBy(x => x).ToArray());
            string authKeysCommaDelimited = string.Join(",", authList.OrderBy(x => x).ToArray());

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGrantV2AccessRequest("GET", "", channelsCommaDelimited, channelGroupsCommaDelimited, targetUuidsCommaDelimited, authKeysCommaDelimited, this.grantRead, this.grantWrite, this.grantDelete, this.grantManage, this.grantGet, this.grantUpdate, this.grantJoin, this.grantTTL, this.queryParam);

            RequestState<PNAccessManagerGrantResult> requestState = new RequestState<PNAccessManagerGrantResult>();
            requestState.Channels = this.pubnubChannelNames;
            requestState.ChannelGroups = this.pubnubChannelGroupNames;
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
            
        }

        internal async Task<PNResult<PNAccessManagerGrantResult>> GrantAccess()
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            PNResult<PNAccessManagerGrantResult> ret = new PNResult<PNAccessManagerGrantResult>();

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            List<string> uuidList = new List<string>();
            List<string> authList = new List<string>();

            if (this.pubnubChannelNames != null && this.pubnubChannelNames.Length > 0)
            {
                channelList = new List<string>(this.pubnubChannelNames);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (this.pubnubChannelGroupNames != null && this.pubnubChannelGroupNames.Length > 0)
            {
                channelGroupList = new List<string>(this.pubnubChannelGroupNames);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (this.pubnubTargetUuids != null && this.pubnubTargetUuids.Length > 0)
            {
                uuidList = new List<string>(this.pubnubTargetUuids);
                uuidList = uuidList.Where(uuid => !string.IsNullOrEmpty(uuid) && uuid.Trim().Length > 0).Distinct().ToList();
            }

            if (this.pamAuthenticationKeys != null && this.pamAuthenticationKeys.Length > 0)
            {
                authList = new List<string>(this.pamAuthenticationKeys);
                authList = authList.Where(auth => !string.IsNullOrEmpty(auth) && auth.Trim().Length > 0).Distinct<string>().ToList();
            }

            string channelsCommaDelimited = string.Join(",", channelList.OrderBy(x => x).ToArray());
            string channelGroupsCommaDelimited = string.Join(",", channelGroupList.OrderBy(x => x).ToArray());
            string targetUuidsCommaDelimited = string.Join(",", uuidList.OrderBy(x => x).ToArray());
            string authKeysCommaDelimited = string.Join(",", authList.OrderBy(x => x).ToArray());

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGrantV2AccessRequest("GET", "", channelsCommaDelimited, channelGroupsCommaDelimited, targetUuidsCommaDelimited,authKeysCommaDelimited, this.grantRead, this.grantWrite, this.grantDelete, this.grantManage, this.grantGet, this.grantUpdate, this.grantJoin, this.grantTTL, this.queryParam);

            RequestState<PNAccessManagerGrantResult> requestState = new RequestState<PNAccessManagerGrantResult>();
            requestState.Channels = this.pubnubChannelNames;
            requestState.ChannelGroups = this.pubnubChannelGroupNames;
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                if (resultList != null && resultList.Count > 0)
                {
                    ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                    PNAccessManagerGrantResult responseResult = responseBuilder.JsonToObject<PNAccessManagerGrantResult>(resultList, true);
                    if (responseResult != null)
                    {
                        ret.Result = responseResult;
                    }
                }
            }
            
            return ret;
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
    }
}
