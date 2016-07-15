using System;
using PubnubApi.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EndPoint
{
    internal class AddChannelsToChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration pnConfig = null;
        private IJsonPluggableLibrary jsonPluggableLibrary = null;

        public AddChannelsToChannelGroupOperation(PNConfiguration pnConfig) : base(pnConfig)
        {
            this.pnConfig = pnConfig;
        }

        public AddChannelsToChannelGroupOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pnConfig, jsonPluggableLibrary)
        {
            this.pnConfig = pnConfig;
            this.jsonPluggableLibrary = jsonPluggableLibrary;
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

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(pnConfig, jsonPluggableLibrary);

            Uri request = urlBuilder.BuildAddChannelsToChannelGroupRequest(channels, nameSpace, groupName);

            RequestState<AddChannelToChannelGroupAck> requestState = new RequestState<AddChannelToChannelGroupAck>();
            requestState.ResponseType = ResponseType.ChannelGroupAdd;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { string.Format("{0}:{1}", nameSpace, groupName) };
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<AddChannelToChannelGroupAck>(request, requestState, false);
        }
    }
}
