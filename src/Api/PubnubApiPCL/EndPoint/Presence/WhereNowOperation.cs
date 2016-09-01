using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class WhereNowOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private string whereNowUUID = "";

        public WhereNowOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public WhereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public WhereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public WhereNowOperation uuid(string uuid)
        {
            this.whereNowUUID = uuid;
            return this;
        }

        public void async(PNCallback<WhereNowAck> callback)
        {
            WhereNow(this.whereNowUUID, callback.result, callback.error);
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

            string json = UrlProcessRequest<WhereNowAck>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<WhereNowAck>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }


    }
}
