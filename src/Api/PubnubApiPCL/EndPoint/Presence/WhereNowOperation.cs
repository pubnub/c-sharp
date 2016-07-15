using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class WhereNowOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        public WhereNowOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            config = pnConfig;
        }

        public WhereNowOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            config = pnConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        internal void WhereNow(string uuid, Action<WhereNowAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }
            if (jsonLibrary == null)
            {
                throw new NullReferenceException("Missing Json Pluggable Library for Pubnub Instance");
            }

            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.Uuid;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildWhereNowRequest(uuid);

            RequestState<WhereNowAck> requestState = new RequestState<WhereNowAck>();
            requestState.Channels = new string[] { uuid };
            requestState.ResponseType = ResponseType.Where_Now;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<WhereNowAck>(request, requestState, false);
        }


    }
}
