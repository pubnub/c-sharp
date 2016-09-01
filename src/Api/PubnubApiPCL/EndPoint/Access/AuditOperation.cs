using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class AuditOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit;
        private string channelName = null;
        private string channelGroupName = null;
        private string[] authenticationKeys = null;

        public AuditOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public AuditOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public AuditOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public AuditOperation channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public AuditOperation channelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public AuditOperation authKeys(string[] authKeys)
        {
            this.authenticationKeys = authKeys;
            return this;
        }

        public void async(PNCallback<AuditAck> callback)
        {
            AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, callback.result, callback.error);
        }

        internal void AuditAccess<T>(string channel, string channelGroup, string[] authKeys, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            string authKeysCommaDelimited = (authKeys != null && authKeys.Length > 0) ? string.Join(",", authKeys) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildAuditAccessRequest(channel, channelGroup, authKeysCommaDelimited);

            RequestState<T> requestState = new RequestState<T>();
            if (!string.IsNullOrEmpty(channel))
            {
                requestState.Channels = new string[] { channel };
            }
            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestState.ChannelGroups = new string[] { channelGroup };
            }
            requestState.ResponseType = ResponseType.AuditAccess;
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
