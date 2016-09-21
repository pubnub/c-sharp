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

        public WhereNowOperation Uuid(string uuid)
        {
            this.whereNowUUID = uuid;
            return this;
        }

        public void Async(PNCallback<PNWhereNowResult> callback)
        {
            WhereNow(this.whereNowUUID, callback);
        }

        internal void WhereNow(string uuid, PNCallback<PNWhereNowResult> callback)
        {
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

            RequestState<PNWhereNowResult> requestState = new RequestState<PNWhereNowResult>();
            requestState.Channels = new string[] { uuid };
            requestState.ResponseType = PNOperationType.PNWhereNowOperation;
            requestState.Callback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNWhereNowResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNWhereNowResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }


    }
}
