using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class HereNowOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private string[] channelNames = null;
        private string[] channelGroupNames = null;
        private bool includeUserState = false;
        private bool includeChannelUUIDs = true;

        public HereNowOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public HereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public HereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public HereNowOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public HereNowOperation ChannelGroups(string[] channelGroups)
        {
            this.channelGroupNames = channelGroups;
            return this;
        }

        public HereNowOperation IncludeState(bool includeState)
        {
            this.includeUserState = includeState;
            return this;
        }

        public HereNowOperation IncludeUUIDs(bool includeUUIDs)
        {
            this.includeChannelUUIDs = includeUUIDs;
            return this;
        }

        public void Async(PNCallback<PNHereNowResult> callback)
        {
            HereNow(this.channelNames, this.channelGroupNames, this.includeChannelUUIDs, this.includeUserState, callback);
        }

        internal void HereNow(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, PNCallback<PNHereNowResult> callback)
        {

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildHereNowRequest(channels, channelGroups, showUUIDList, includeUserState);

            RequestState<PNHereNowResult> requestState = new RequestState<PNHereNowResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNHereNowOperation;
            requestState.Reconnect = false;
            requestState.PubnubCallback = callback;

            string json = UrlProcessRequest<PNHereNowResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNHereNowResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        

    }
}
