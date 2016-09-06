using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class AddChannelsToChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        private string channelGroupName = "";
        private string[] channelNames = null;

        public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public AddChannelsToChannelGroupOperation channelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public AddChannelsToChannelGroupOperation channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public void async(PNCallback<PNChannelGroupsAddChannelResult> callback)
        {
            AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, callback.result, callback.error);
        }

        internal void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Action<PNChannelGroupsAddChannelResult> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Missing channel(s)");
            }

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

            string channelsCommaDelimited = channels != null && channels.Length > 0 ? string.Join(",", channels) : "";

            Uri request = urlBuilder.BuildAddChannelsToChannelGroupRequest(channelsCommaDelimited, nameSpace, groupName);

            RequestState<PNChannelGroupsAddChannelResult> requestState = new RequestState<PNChannelGroupsAddChannelResult>();
            requestState.ResponseType = ResponseType.ChannelGroupAdd;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNChannelGroupsAddChannelResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsAddChannelResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
