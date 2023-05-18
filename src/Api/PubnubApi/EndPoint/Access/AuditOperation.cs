using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class AuditOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string channelName;
        private string channelGroupName;
        private string[] authenticationKeys;
        private PNCallback<PNAccessManagerAuditResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public AuditOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, null, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public AuditOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public AuditOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public AuditOperation AuthKeys(string[] authKeys)
        {
            this.authenticationKeys = authKeys;
            return this;
        }

        public AuditOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNAccessManagerAuditResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNAccessManagerAuditResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }


        public async Task<PNResult<PNAccessManagerAuditResult>> ExecuteAsync()
        {
            return await AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void AuditAccess(string channel, string channelGroup, string[] authKeys, Dictionary<string, object> externalQueryParam, PNCallback<PNAccessManagerAuditResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            string authKeysCommaDelimited = (authKeys != null && authKeys.Length > 0) ? string.Join(",", authKeys.OrderBy(x => x).ToArray()) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildAuditAccessRequest("GET", "", channel, channelGroup, authKeysCommaDelimited, externalQueryParam);

            RequestState<PNAccessManagerAuditResult> requestState = new RequestState<PNAccessManagerAuditResult>();
            if (!string.IsNullOrEmpty(channel))
            {
                requestState.Channels = new [] { channel };
            }
            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestState.ChannelGroups = new [] { channelGroup };
            }
            requestState.ResponseType = PNOperationType.PNAccessManagerAudit;
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

        internal async Task<PNResult<PNAccessManagerAuditResult>> AuditAccess(string channel, string channelGroup, string[] authKeys, Dictionary<string, object> externalQueryParam)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            PNResult<PNAccessManagerAuditResult> ret = new PNResult<PNAccessManagerAuditResult>();

            string authKeysCommaDelimited = (authKeys != null && authKeys.Length > 0) ? string.Join(",", authKeys.OrderBy(x => x).ToArray()) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildAuditAccessRequest("GET", "", channel, channelGroup, authKeysCommaDelimited, externalQueryParam);

            RequestState<PNAccessManagerAuditResult> requestState = new RequestState<PNAccessManagerAuditResult>();
            if (!string.IsNullOrEmpty(channel))
            {
                requestState.Channels = new[] { channel };
            }
            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestState.ChannelGroups = new[] { channelGroup };
            }
            requestState.ResponseType = PNOperationType.PNAccessManagerAudit;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse(requestState, json);
                if (result != null)
                {
                    List<object> resultList = ProcessJsonResponse(requestState, json);
                    if (resultList != null && resultList.Count > 0)
                    {
                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                        PNAccessManagerAuditResult responseResult = responseBuilder.JsonToObject<PNAccessManagerAuditResult>(resultList, true);
                        if (responseResult != null)
                        {
                            ret.Result = responseResult;
                        }
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
