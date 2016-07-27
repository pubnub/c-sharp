using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    internal class GetAllChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonPluggableLibrary = null;

        public GetAllChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            this.pubnubConfig = pubnubConfig;
        }

        public GetAllChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            this.pubnubConfig = pubnubConfig;
            this.jsonPluggableLibrary = jsonPluggableLibrary;
        }

        public GetAllChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            this.pubnubConfig = pubnubConfig;
        }

        internal void GetAllChannelGroup(Action<GetAllChannelGroupsAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(pubnubConfig, jsonPluggableLibrary);

            Uri request = urlBuilder.BuildGetAllChannelGroupRequest();

            RequestState<GetAllChannelGroupsAck> requestState = new RequestState<GetAllChannelGroupsAck>();
            requestState.ResponseType = ResponseType.ChannelGroupGet;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<GetAllChannelGroupsAck>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = base.ProcessJsonResponse<GetAllChannelGroupsAck>(requestState, json);
                base.ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
