using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace PubnubApi.EndPoint
{
    public class CreateSpaceOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string spcId = "";
        private string spcName = "";
        private string spcDesc;
        private Dictionary<string, object> spcCustom;

        private PNCallback<PNCreateSpaceResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public CreateSpaceOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

            if (instance != null)
            {
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

        public CreateSpaceOperation Id(string spaceId)
        {
            this.spcId = spaceId;
            return this;
        }

        public CreateSpaceOperation Name(string spaceName)
        {
            this.spcName = spaceName;
            return this;
        }

        public CreateSpaceOperation Description(string spaceDescription)
        {
            this.spcDesc = spaceDescription;
            return this;
        }

        public CreateSpaceOperation CustomObject(Dictionary<string, object> spaceCustomObject)
        {
            this.spcCustom = spaceCustomObject;
            return this;
        }

        public CreateSpaceOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNCreateSpaceResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                CreateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                CreateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                CreateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                CreateSpace(this.spcId, this.spcName, this.spcDesc, this.spcCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void CreateSpace(string spaceId, string spaceName, string spaceDescription, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNCreateSpaceResult> callback)
        {
            if (string.IsNullOrEmpty(spaceId) || string.IsNullOrEmpty(spaceId.Trim()) || spaceName == null)
            {
                throw new ArgumentException("Missing Id or Name");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }


            RequestState<PNCreateSpaceResult> requestState = new RequestState<PNCreateSpaceResult>();
            requestState.ResponseType = PNOperationType.PNCreateSpaceOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("id", spaceId);
            messageEnvelope.Add("name", spaceName);
            if (spaceDescription != null)
            {
                messageEnvelope.Add("description", spaceDescription);
            }
            if (spaceCustom != null)
            {
                messageEnvelope.Add("custom", spaceCustom);
            }
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildCreateSpaceRequest("POST", postMessage, spaceCustom, externalQueryParam);

            string json = UrlProcessRequest<PNCreateSpaceResult>(request, requestState, false, postMessage);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNCreateSpaceResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
