using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class AddChannelsToChannelGroupOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pnTelemetryMgr;

        private string channelGroupName = "";
        private string[] channelNames;
        private PNCallback<PNChannelGroupsAddChannelResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pnTelemetryMgr = telemetryManager;
        }

        public AddChannelsToChannelGroupOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public AddChannelsToChannelGroupOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public AddChannelsToChannelGroupOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNChannelGroupsAddChannelResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNChannelGroupsAddChannelResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNChannelGroupsAddChannelResult>> ExecuteAsync()
        {
            return await AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam, PNCallback<PNChannelGroupsAddChannelResult> callback)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Missing channel(s)");
            }

            if (nameSpace == null)
            {
                throw new ArgumentException("Missing nameSpace");
            }

            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pnTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            string channelsCommaDelimited = string.Join(",", channels.OrderBy(x => x).ToArray());

            Uri request = urlBuilder.BuildAddChannelsToChannelGroupRequest("GET", "", channelsCommaDelimited, nameSpace, groupName, externalQueryParam);

            RequestState<PNChannelGroupsAddChannelResult> requestState = new RequestState<PNChannelGroupsAddChannelResult>();
            requestState.ResponseType = PNOperationType.PNAddChannelsToGroupOperation;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new [] { groupName };
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

        internal async Task<PNResult<PNChannelGroupsAddChannelResult>> AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Missing channel(s)");
            }

            if (nameSpace == null)
            {
                throw new ArgumentException("Missing nameSpace");
            }

            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }
            PNResult<PNChannelGroupsAddChannelResult> ret = new PNResult<PNChannelGroupsAddChannelResult>();

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pnTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            string channelsCommaDelimited = string.Join(",", channels.OrderBy(x => x).ToArray());

            Uri request = urlBuilder.BuildAddChannelsToChannelGroupRequest("GET", "", channelsCommaDelimited, nameSpace, groupName, externalQueryParam);

            RequestState<PNChannelGroupsAddChannelResult> requestState = new RequestState<PNChannelGroupsAddChannelResult>();
            requestState.ResponseType = PNOperationType.PNAddChannelsToGroupOperation;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new[] { groupName };
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNChannelGroupsAddChannelResult responseResult = responseBuilder.JsonToObject<PNChannelGroupsAddChannelResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
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
