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
        private IPubnubUnitTest unitTest;
        private PNPushType pubnubPushType;
        private string[] channelNames = null;
        private string deviceTokenId = "";

        public RemovePushChannelOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public RemovePushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public RemovePushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
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
            RemoveChannelForDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, callback.Result, callback.Error);
        }

        internal void RemoveChannelForDevice(string[] channels, PNPushType pushType, string pushToken, Action<PNPushRemoveChannelResult> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }

            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            string channel = string.Join(",", channels);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unitTest);
            Uri request = urlBuilder.BuildRemoveChannelPushRequest(channel, pushType, pushToken);

            RequestState<PNPushRemoveChannelResult> requestState = new RequestState<PNPushRemoveChannelResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.PushRemove;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNPushRemoveChannelResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPushRemoveChannelResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }



    }
}
