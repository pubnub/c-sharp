using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    internal class AddChannelsToChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

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

        internal void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Action<AddChannelToChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
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

            RequestState<AddChannelToChannelGroupAck> requestState = new RequestState<AddChannelToChannelGroupAck>();
            requestState.ResponseType = ResponseType.ChannelGroupAdd;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<AddChannelToChannelGroupAck>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = base.ProcessJsonResponse<AddChannelToChannelGroupAck>(requestState, json);
                base.ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
