using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class AuditPushChannelOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unitTest;
        private PNPushType pubnubPushType;
        private string deviceTokenId = "";

        public AuditPushChannelOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public AuditPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public AuditPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
        }

        public AuditPushChannelOperation PushType(PNPushType pushType)
        {
            this.pubnubPushType = pushType;
            return this;
        }

        public AuditPushChannelOperation DeviceId(string deviceId)
        {
            this.deviceTokenId = deviceId;
            return this;
        }

        public void Async(PNCallback<PNPushListProvisionsResult> callback)
        {
            GetChannelsForDevice(this.pubnubPushType, this.deviceTokenId, callback.Result, callback.Error);
        }

        internal void GetChannelsForDevice(PNPushType pushType, string pushToken, Action<PNPushListProvisionsResult> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unitTest);
            Uri request = urlBuilder.BuildGetChannelsPushRequest(pushType, pushToken);

            RequestState<PNPushListProvisionsResult> requestState = new RequestState<PNPushListProvisionsResult>();
            requestState.ResponseType = ResponseType.PushGet;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNPushListProvisionsResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPushListProvisionsResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
