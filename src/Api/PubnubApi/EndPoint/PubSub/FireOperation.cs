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
    public class FireOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private object msg;
        private string channelName = "";
        private bool httpPost;
        private Dictionary<string, object> userMetadata;
        private readonly int ttl = -1;
        private PNCallback<PNPublishResult> savedCallback;
        private bool syncRequest;
        private Dictionary<string, object> queryParam;

        public FireOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public FireOperation Message(object message)
        {
            this.msg = message;
            return this;
        }

        public FireOperation Channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public FireOperation Meta(Dictionary<string, object> metadata)
        {
            this.userMetadata = metadata;
            return this;
        }

        public FireOperation UsePOST(bool post)
        {
            this.httpPost = post;
            return this;
        }

        public FireOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Async(PNCallback<PNPublishResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                syncRequest = false;
                this.savedCallback = callback;
                Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                syncRequest = false;
                this.savedCallback = callback;
                Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public PNPublishResult Sync()
        {
            System.Threading.ManualResetEvent syncEvent = new System.Threading.ManualResetEvent(false);
            Task<PNPublishResult> task = Task<PNPublishResult>.Factory.StartNew(() =>
            {
                syncRequest = true;
                syncEvent = new System.Threading.ManualResetEvent(false);
                Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, new PNPublishResultExt((r,s)=> { SyncResult = r; syncEvent.Set(); }));
                syncEvent.WaitOne(config.NonSubscribeRequestTimeout * 1000);

                return SyncResult;
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            return task.Result;
        }

        private static PNPublishResult SyncResult { get; set; }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                if (!syncRequest)
                {
                    Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, savedCallback);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                if (!syncRequest)
                {
                    Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, savedCallback);
                }
            })
            { IsBackground = true }.Start();
#endif
        }

        private void Fire(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid publish key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

            Dictionary<string, string> urlParam = new Dictionary<string, string>();
            urlParam.Add("norep", "true");

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildPublishRequest(channel, message, storeInHistory, ttl, metaData, httpPost, urlParam, externalQueryParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new [] { channel };
            requestState.ResponseType = PNOperationType.PNFireOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            if (this.httpPost)
            {
                requestState.UsePostMethod = true;
                Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
                messageEnvelope.Add("message", message);
                string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
                json = UrlProcessRequest<PNPublishResult>(request, requestState, false, postMessage);
            }
            else
            {
                json = UrlProcessRequest<PNPublishResult>(request, requestState, false);
            }

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPublishResult>(requestState, json);
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
    }

}
