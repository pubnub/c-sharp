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
        private IPubnubUnitTest unit = null;

        private string whereNowUUID = "";
        private PNCallback<PNWhereNowResult> savedCallback = null;

        public WhereNowOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public WhereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public WhereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public WhereNowOperation Uuid(string uuid)
        {
            this.whereNowUUID = uuid;
            return this;
        }

        public void Async(PNCallback<PNWhereNowResult> callback)
        {
            this.savedCallback = callback;
            WhereNow(this.whereNowUUID, callback);
        }

        internal void Retry()
        {
            WhereNow(this.whereNowUUID, savedCallback);
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

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildWhereNowRequest(uuid);

            RequestState<PNWhereNowResult> requestState = new RequestState<PNWhereNowResult>();
            requestState.Channels = new string[] { uuid };
            requestState.ResponseType = PNOperationType.PNWhereNowOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNWhereNowResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNWhereNowResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }


    }
}
