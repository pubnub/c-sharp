using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class GlobalHereNowOperation : PubnubCoreBase
    {
        private PNConfiguration _pnConfig = null;
        private IJsonPluggableLibrary _jsonPluggableLibrary = null;

        public GlobalHereNowOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            _pnConfig = pnConfig;
        }

        public GlobalHereNowOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            _pnConfig = pnConfig;
            _jsonPluggableLibrary = jsonPluggableLibrary;
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

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildGlobalHereNowRequest(showUUIDList, includeUserState);

            RequestState<GlobalHereNowAck> requestState = new RequestState<GlobalHereNowAck>();
            requestState.Channels = null;
            requestState.ResponseType = ResponseType.GlobalHere_Now;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<GlobalHereNowAck>(request, requestState);
        }

        
    }
}
