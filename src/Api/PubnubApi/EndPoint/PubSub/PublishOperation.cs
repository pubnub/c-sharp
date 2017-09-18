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
    public class PublishOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr = null;

        private object msg = null;
        private string channelName = "";
        private bool storeInHistory = true;
        private bool httpPost = false;
        private Dictionary<string, object> userMetadata = null;
        private int ttl = -1;
        private PNCallback<PNPublishResult> savedCallback = null;
        private bool syncRequest = false;

        public PublishOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }


        public PublishOperation Message(object message)
        {
            this.msg = message;
            return this;
        }

        public PublishOperation Channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public PublishOperation ShouldStore(bool store)
        {
            this.storeInHistory = store;
            return this;
        }

        public PublishOperation Meta(Dictionary<string, object> metadata)
        {
            this.userMetadata = metadata;
            return this;
        }

        public PublishOperation UsePOST(bool post)
        {
            this.httpPost = post;
            return this;
        }

        /// <summary>
        /// tttl in hours
        /// </summary>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public PublishOperation Ttl(int ttl)
        {
            this.ttl = ttl;
            return this;
        }

        public void Async(PNCallback<PNPublishResult> callback)
        {
            if (this.msg == null)
            {
                throw new ArgumentException("message cannot be null");
            }

            if (this.config == null || string.IsNullOrEmpty(this.config.PublishKey) || this.config.PublishKey.Trim().Length <= 0)
            {
                throw new MissingMemberException("publish key is required");
            }

            Task.Factory.StartNew(() =>
            {
                syncRequest = false;
                this.savedCallback = callback;
                Publish(this.channelName, this.msg, this.storeInHistory, this.ttl, this.userMetadata, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private static System.Threading.ManualResetEvent syncEvent = new System.Threading.ManualResetEvent(false);
        public PNPublishResult Sync()
        {
            if (this.msg == null)
            {
                throw new ArgumentException("message cannot be null");
            }
            Task<PNPublishResult> task = Task<PNPublishResult>.Factory.StartNew(() =>
            {
                syncRequest = true;
                syncEvent = new System.Threading.ManualResetEvent(false);
                Publish(this.channelName, this.msg, this.storeInHistory, this.ttl, this.userMetadata, new SyncPublishResult());
                syncEvent.WaitOne(config.NonSubscribeRequestTimeout * 1000);

                return SyncResult;
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            return task.Result;
        }

        private static PNPublishResult SyncResult { get; set; }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                if (!syncRequest)
                {
                    Publish(this.channelName, this.msg, this.storeInHistory, this.ttl, this.userMetadata, savedCallback);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private void Publish(string channel, object message, bool storeInHistory, int ttl, Dictionary<string,object> metaData, PNCallback<PNPublishResult> callback)
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
            Uri request = urlBuilder.BuildPublishRequest(channel, message, storeInHistory, ttl, metaData, httpPost, null);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PNPublishOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            if (this.httpPost)
            {
                requestState.UsePostMethod = true;
                string postMessage = JsonEncodePublishMsg(message);
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

        private class SyncPublishResult : PNCallback<PNPublishResult>
        {
            public override void OnResponse(PNPublishResult result, PNStatus status)
            {
                SyncResult = result;
                syncEvent.Set();
            }
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.Add(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
        }

        private string JsonEncodePublishMsg(object originalMessage)
        {
            string message = jsonLibrary.SerializeToJsonString(originalMessage);

            if (config.CipherKey.Length > 0)
            {
                PubnubCrypto aes = new PubnubCrypto(config.CipherKey, config, pubnubLog);
                string encryptMessage = aes.Encrypt(message);
                message = jsonLibrary.SerializeToJsonString(encryptMessage);
            }

            return message;
        }
    }
}
