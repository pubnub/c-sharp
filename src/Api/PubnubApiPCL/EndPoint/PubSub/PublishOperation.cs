using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class PublishOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        private object msg = null;
        private string channelName = "";
        private bool storeInHistory = true;
        private bool httpPost = false;
        private string userMetadata = "";

        public PublishOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public PublishOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public PublishOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public PublishOperation message(object message)
        {
            this.msg = message;
            return this;
        }

        public PublishOperation channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public PublishOperation shouldStore(bool store)
        {
            this.storeInHistory = store;
            return this;
        }

        public PublishOperation meta(string jsonMetadata)
        {
            this.userMetadata = jsonMetadata;
            return this;
        }

        public PublishOperation usePOST(bool post)
        {
            this.httpPost = post;
            return this;
        }

        public void async(PNCallback<PublishAck> callback)
        {
            Publish(this.channelName, this.msg, this.storeInHistory, this.userMetadata, callback.result, callback.error);
        }

        private void Publish(string channel, object message, bool storeInHistory, string jsonUserMetaData, Action<PublishAck> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
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

            if (config.EnableDebugForPushPublish)
            {
                if (message is Dictionary<string, object>)
                {
                    Dictionary<string, object> dicMessage = message as Dictionary<string, object>;
                    dicMessage.Add("pn_debug", true);
                    message = dicMessage;
                }
            }

            if (string.IsNullOrEmpty(jsonUserMetaData) || jsonLibrary.IsDictionaryCompatible(jsonUserMetaData))
            {
                jsonUserMetaData = "";
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildPublishRequest(channel, message, storeInHistory, jsonUserMetaData);

            RequestState<PublishAck> requestState = new RequestState<PublishAck>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.Publish;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PublishAck>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PublishAck>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
