using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class DeleteChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        private string channelGroupName = "";

        public DeleteChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public DeleteChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public DeleteChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public DeleteChannelGroupOperation channelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public void async(PNCallback<PNChannelGroupsDeleteGroupResult> callback)
        {
            DeleteChannelGroup(this.channelGroupName, callback.result, callback.error);
        }

        internal void DeleteChannelGroup(string groupName, Action<PNChannelGroupsDeleteGroupResult> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }

            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);

            Uri request = urlBuilder.BuildRemoveChannelsFromChannelGroupRequest(null, "", groupName);

            RequestState<PNChannelGroupsDeleteGroupResult> requestState = new RequestState<PNChannelGroupsDeleteGroupResult>();
            requestState.ResponseType = ResponseType.ChannelGroupRemove;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { groupName };
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNChannelGroupsDeleteGroupResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsDeleteGroupResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
