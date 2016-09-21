using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class ListChannelsForChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private string channelGroupName = "";

        public ListChannelsForChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public ListChannelsForChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public ListChannelsForChannelGroupOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public void Async(PNCallback<PNChannelGroupsAllChannelsResult> callback)
        {
            GetChannelsForChannelGroup(this.channelGroupName, callback);
        }

        internal void GetChannelsForChannelGroup(string groupName, PNCallback<PNChannelGroupsAllChannelsResult> callback)
        {
            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);

            Uri request = urlBuilder.BuildGetChannelsForChannelGroupRequest(null, groupName, false);

            RequestState<PNChannelGroupsAllChannelsResult> requestState = new RequestState<PNChannelGroupsAllChannelsResult>();
            requestState.ResponseType = PNOperationType.ChannelGroupGet;
            requestState.ChannelGroups = new string[] { groupName };
            requestState.Callback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNChannelGroupsAllChannelsResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsAllChannelsResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        internal void GetChannelsForChannelGroup<T>(string nameSpace, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (nameSpace == null)
            {
                throw new ArgumentException("Missing nameSpace");
            }

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

            Uri request = urlBuilder.BuildGetChannelsForChannelGroupRequest(nameSpace, groupName, false);

            RequestState<T> requestState = new RequestState<T>();
            requestState.ResponseType = PNOperationType.ChannelGroupGet;
            requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<T>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<T>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
