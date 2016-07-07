using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class PublishOperation : PubnubCoreBase
    {
        private PNConfiguration _pnConfig = null;
        private IJsonPluggableLibrary _jsonPluggableLibrary = null;

        public PublishOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            _pnConfig = pnConfig;
        }

        public PublishOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            _pnConfig = pnConfig;
            _jsonPluggableLibrary = jsonPluggableLibrary;
        }

        internal void Publish(string channel, object message, bool storeInHistory, string jsonUserMetaData, Action<PublishAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(_pnConfig.PublishKey) || string.IsNullOrEmpty(_pnConfig.PublishKey.Trim()) || _pnConfig.PublishKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid publish key");
            }

            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            if (_pnConfig.EnableDebugForPushPublish)
            {
                if (message is Dictionary<string, object>)
                {
                    Dictionary<string, object> dicMessage = message as Dictionary<string, object>;
                    dicMessage.Add("pn_debug", true);
                    message = dicMessage;
                }
            }

            if (string.IsNullOrEmpty(jsonUserMetaData) || _jsonPluggableLibrary.IsDictionaryCompatible(jsonUserMetaData))
            {
                jsonUserMetaData = "";
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildPublishRequest(channel, message, storeInHistory, jsonUserMetaData);

            RequestState<PublishAck> requestState = new RequestState<PublishAck>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.Publish;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<PublishAck>(request, requestState);
        }

    }
}
