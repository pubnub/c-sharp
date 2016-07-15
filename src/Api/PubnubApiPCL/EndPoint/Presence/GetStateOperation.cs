using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class GetStateOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        public GetStateOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            config = pnConfig;
        }

        public GetStateOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            config = pnConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        internal void GetUserState(string[] channels, string[] channelGroups, string uuid, Action<GetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if ((channels == null && channelGroups != null) || (channels.Length == 0  && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.Uuid;
            }

            string channelsCommaDelimited = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";
            string channelGroupsCommaDelimited = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildGetUserStateRequest(channelsCommaDelimited, channelGroupsCommaDelimited, uuid);

            RequestState<GetUserStateAck> requestState = new RequestState<GetUserStateAck>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = ResponseType.GetUserState;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<GetUserStateAck>(request, requestState, false);
        }
    }
}
