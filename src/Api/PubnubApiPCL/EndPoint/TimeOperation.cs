using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class TimeOperation: PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;

        public TimeOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public TimeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public void Async(PNCallback<PNTimeResult> callback)
        {
            Time(callback);
        }

        internal void Time(PNCallback<PNTimeResult> callback)
        {
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config);
            Uri request = urlBuilder.BuildTimeRequest();

            RequestState<PNTimeResult> requestState = new RequestState<PNTimeResult>();
            requestState.Channels = null;
            requestState.ResponseType = PNOperationType.PNTimeOperation;
            requestState.Callback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNTimeResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNTimeResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
