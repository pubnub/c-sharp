using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class TimeOperation: PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private static IPubnubUnitTest unitTest = null;

        public TimeOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public TimeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public TimeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
        }

        internal void Time(Action<long> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config);
            Uri request = urlBuilder.BuildTimeRequest();

            RequestState<long> requestState = new RequestState<long>();
            requestState.Channels = null;
            requestState.ResponseType = ResponseType.Time;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<long>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<long>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
            else
            {
                TimeExceptionHandler(errorCallback);
            }
        }

        private void TimeExceptionHandler(Action<PubnubClientError> errorCallback)
        {
            string message = "Operation Timeout or Network connnect error";

            LoggingMethod.WriteToLog(string.Format("DateTime {0}, TimeExceptionHandler response={1}", DateTime.Now.ToString(), message), LoggingMethod.LevelInfo);

            new PNCallbackService(config, jsonLibrary).CallErrorCallback(PubnubErrorSeverity.Critical, PubnubMessageSource.Client,
                "", "", errorCallback, message, PubnubErrorCode.TimeOperationTimeout, null, null);
        }

    }
}
