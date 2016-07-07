using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class HereNowOperation : PubnubCoreBase
    {
        private PNConfiguration _pnConfig = null;
        private IJsonPluggableLibrary _jsonPluggableLibrary = null;

        public HereNowOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            _pnConfig = pnConfig;
        }

        public HereNowOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            _pnConfig = pnConfig;
            _jsonPluggableLibrary = jsonPluggableLibrary;
        }

        internal void HereNow(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Action<HereNowAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if ((channels == null && channelGroups == null) || (channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new ArgumentException("Missing Channel/ChannelGroup");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildHereNowRequest(channels, channelGroups, showUUIDList, includeUserState);

            RequestState<HereNowAck> requestState = new RequestState<HereNowAck>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = ResponseType.Here_Now;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<HereNowAck>(request, requestState);
        }

        

    }
}
