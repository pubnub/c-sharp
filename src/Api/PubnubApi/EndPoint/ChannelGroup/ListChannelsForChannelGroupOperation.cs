using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class ListChannelsForChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private string channelGroupName = "";
        private PNCallback<PNChannelGroupsAllChannelsResult> savedCallback = null;

        public ListChannelsForChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public ListChannelsForChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public ListChannelsForChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }


        public ListChannelsForChannelGroupOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public void Async(PNCallback<PNChannelGroupsAllChannelsResult> callback)
        {
            this.savedCallback = callback;
            GetChannelsForChannelGroup(this.channelGroupName, callback);
        }

        internal void Retry()
        {
            GetChannelsForChannelGroup(this.channelGroupName, savedCallback);
        }

        internal void GetChannelsForChannelGroup(string groupName, PNCallback<PNChannelGroupsAllChannelsResult> callback)
        {
            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);

            Uri request = urlBuilder.BuildGetChannelsForChannelGroupRequest(null, groupName, false);

            RequestState<PNChannelGroupsAllChannelsResult> requestState = new RequestState<PNChannelGroupsAllChannelsResult>();
            requestState.ResponseType = PNOperationType.ChannelGroupGet;
            requestState.ChannelGroups = new string[] { groupName };
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNChannelGroupsAllChannelsResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsAllChannelsResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
