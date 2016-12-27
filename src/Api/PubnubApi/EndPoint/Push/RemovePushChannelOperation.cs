using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class RemovePushChannelOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private PNPushType pubnubPushType;
        private string[] channelNames = null;
        private string deviceTokenId = "";
        private PNCallback<PNPushRemoveChannelResult> savedCallback = null;

        public RemovePushChannelOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public RemovePushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public RemovePushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public RemovePushChannelOperation PushType(PNPushType pushType)
        {
            this.pubnubPushType = pushType;
            return this;
        }

        public RemovePushChannelOperation DeviceId(string deviceId)
        {
            this.deviceTokenId = deviceId;
            return this;
        }

        public RemovePushChannelOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public void Async(PNCallback<PNPushRemoveChannelResult> callback)
        {
            this.savedCallback = callback;
            RemoveChannelForDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, callback);
        }

        internal void Retry()
        {
            RemoveChannelForDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, savedCallback);
        }

        internal void RemoveChannelForDevice(string[] channels, PNPushType pushType, string pushToken, PNCallback<PNPushRemoveChannelResult> callback)
        {
            if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }

            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }

            string channel = string.Join(",", channels);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildRemoveChannelPushRequest(channel, pushType, pushToken);

            RequestState<PNPushRemoveChannelResult> requestState = new RequestState<PNPushRemoveChannelResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PushRemove;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNPushRemoveChannelResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPushRemoveChannelResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }



    }
}
