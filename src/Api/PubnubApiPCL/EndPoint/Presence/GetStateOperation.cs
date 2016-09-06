using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class GetStateOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private string[] channelNames = null;
        private string[] channelGroupNames = null;
        private string channelUUID = "";

        public GetStateOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public GetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public GetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public GetStateOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public GetStateOperation ChannelGroups(string[] channelGroups)
        {
            this.channelGroupNames = channelGroups;
            return this;
        }

        public GetStateOperation Uuid(string uuid)
        {
            this.channelUUID = uuid;
            return this;
        }

        public void Async(PNCallback<PNGetStateResult> callback)
        {
            GetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, callback.Result, callback.Error);
        }

        internal void GetUserState(string[] channels, string[] channelGroups, string uuid, Action<PNGetStateResult> userCallback, Action<PubnubClientError> errorCallback)
        {
            if ((channels == null && channelGroups == null)
                           || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided");
            }
            if ((channels == null && channelGroups != null) || (channels.Length == 0  && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            if (string.IsNullOrEmpty(uuid) || uuid.Trim().Length == 0)
            {
                uuid = config.Uuid;
            }

            string channelsCommaDelimited = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";
            string channelGroupsCommaDelimited = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildGetUserStateRequest(channelsCommaDelimited, channelGroupsCommaDelimited, uuid);

            RequestState<PNGetStateResult> requestState = new RequestState<PNGetStateResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = ResponseType.GetUserState;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNGetStateResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNGetStateResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
