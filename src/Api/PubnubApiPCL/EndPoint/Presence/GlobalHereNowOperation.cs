using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class GlobalHereNowOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        public GlobalHereNowOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            config = pnConfig;
        }

        public GlobalHereNowOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            config = pnConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        internal void GlobalHereNow(Action<GlobalHereNowAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            GlobalHereNow(true, false, userCallback, errorCallback);
        }

        internal void GlobalHereNow(bool showUUIDList, bool includeUserState, Action<GlobalHereNowAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildGlobalHereNowRequest(showUUIDList, includeUserState);

            RequestState<GlobalHereNowAck> requestState = new RequestState<GlobalHereNowAck>();
            requestState.Channels = null;
            requestState.ResponseType = ResponseType.GlobalHere_Now;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<GlobalHereNowAck>(request, requestState, false);
        }

        
    }
}
