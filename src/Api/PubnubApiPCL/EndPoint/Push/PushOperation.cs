using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class PushOperation: PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unitTest;

        public PushOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public PushOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public PushOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
        }

        public void RegisterDevice<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }
            if (pushType == PushTypeService.None)
            {
                throw new ArgumentException("Missing PushTypeService");
            }
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unitTest);
            Uri request = urlBuilder.BuildRegisterDevicePushRequest(channel, pushType, pushToken);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.PushRegister;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<T>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<T>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        public void UnregisterDevice<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (pushType == PushTypeService.None)
            {
                throw new ArgumentException("Missing PushTypeService");
            }
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unitTest);
            Uri request = urlBuilder.BuildUnregisterDevicePushRequest(pushType, pushToken);

            RequestState<T> requestState = new RequestState<T>();
            requestState.ResponseType = ResponseType.PushUnregister;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<T>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<T>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        public void RemoveChannelForDevice<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }
            if (pushType == PushTypeService.None)
            {
                throw new ArgumentException("Missing PushTypeService");
            }
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unitTest);
            Uri request = urlBuilder.BuildRemoveChannelPushRequest(channel, pushType, pushToken);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.PushRemove;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<T>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<T>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        public void GetChannelsForDevice<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (pushType == PushTypeService.None)
            {
                throw new ArgumentException("Missing PushTypeService");
            }
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unitTest);
            Uri request = urlBuilder.BuildGetChannelsPushRequest(pushType, pushToken);

            RequestState<T> requestState = new RequestState<T>();
            requestState.ResponseType = ResponseType.PushGet;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<T>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<T>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
