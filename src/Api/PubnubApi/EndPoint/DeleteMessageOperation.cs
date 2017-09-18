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
    public class DeleteMessageOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr;

        private long startTimetoken = -1;
        private long endTimetoken = -1;

        private string channelName = "";
        private PNCallback<PNDeleteMessageResult> savedCallback = null;

        public DeleteMessageOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public DeleteMessageOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public DeleteMessageOperation Start(long start)
        {
            this.startTimetoken = start;
            return this;
        }

        public DeleteMessageOperation End(long end)
        {
            this.endTimetoken = end;
            return this;
        }

        public void Async(PNCallback<PNDeleteMessageResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length == 0)
            {
                throw new MissingMemberException("Invalid Subscribe Key");
            }
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                DeleteMessage(this.channelName, this.startTimetoken, this.endTimetoken, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                DeleteMessage(this.channelName, this.startTimetoken, this.endTimetoken, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void DeleteMessage(string channel, long start, long end, PNCallback<PNDeleteMessageResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildDeleteMessageRequest(channel, start, end);

            RequestState<PNDeleteMessageResult> requestState = new RequestState<PNDeleteMessageResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PNDeleteMessageOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNDeleteMessageResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                //json = json.Replace("\"error\": False", "\"error\": false"); //THIS IS A HACK UNTIL IT IS FIXED AT SERVER
                List<object> result = ProcessJsonResponse<PNDeleteMessageResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
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
    }
}
