using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class RemoveChannelsFromChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        private string channelGroupName = "";
        private string[] channelNames = null;

        public RemoveChannelsFromChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public RemoveChannelsFromChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public RemoveChannelsFromChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public RemoveChannelsFromChannelGroupOperation channelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public RemoveChannelsFromChannelGroupOperation channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public void async(PNCallback<PNChannelGroupsRemoveChannelResult> callback)
        {
            RemoveChannelsFromChannelGroup(this.channelNames, "", this.channelGroupName, callback.result, callback.error);
        }

        internal void RemoveChannelsFromChannelGroup(string[] channels, string nameSpace, string groupName, Action<PNChannelGroupsRemoveChannelResult> userCallback, Action<PubnubClientError> errorCallback)
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


            Uri request = urlBuilder.BuildRemoveChannelsFromChannelGroupRequest(channelsCommaDelimited, nameSpace, groupName);

            RequestState<PNChannelGroupsRemoveChannelResult> requestState = new RequestState<PNChannelGroupsRemoveChannelResult>();
            requestState.ResponseType = ResponseType.ChannelGroupRemove;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNChannelGroupsRemoveChannelResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsRemoveChannelResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
