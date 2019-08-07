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
    public class SignalOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private object msg;
        private string channelName = "";
        private readonly bool httpPost = false;
        private readonly Dictionary<string, object> userMetadata = null;
        private PNCallback<PNPublishResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SignalOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
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

        public SignalOperation Message(object message)
        {
            this.msg = message;
            return this;
        }

        public SignalOperation Channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public SignalOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNPublishResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                Signal(this.channelName, this.msg, this.userMetadata, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                Signal(this.channelName, this.msg, this.userMetadata, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                Signal(this.channelName, this.msg, this.userMetadata, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                Signal(this.channelName, this.msg, this.userMetadata, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void Signal(string channel, object message, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
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


            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildSignalRequest(channel, message, metaData, httpPost, externalQueryParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNSignalOperation;
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

                if (result != null && result.Count >= 3)
                {
                    int signalStatus;
                    Int32.TryParse(result[0].ToString(), out signalStatus);
                    if (signalStatus == 1)
                    {
                        ProcessResponseCallbacks(result, requestState);
                    }
                    else
                    {
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNSignalOperation, category, requestState, 400, new PNException(json));
                        if (requestState != null && requestState.PubnubCallback != null)
                        {
                            requestState.PubnubCallback.OnResponse(default(PNPublishResult), status);
                        }
                        else
                        {
                            Announce(status);
                        }
                    }
                }
                else
                {
                    ProcessResponseCallbacks(result, requestState);
                }
            }
        }
    }
}
