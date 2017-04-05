using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;

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
        private PNCallback<PNHereNowResult> savedCallback = null;

        public HereNowOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public HereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public HereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
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
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                HereNow(this.channelNames, this.channelGroupNames, this.includeChannelUUIDs, this.includeUserState, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                HereNow(this.channelNames, this.channelGroupNames, this.includeChannelUUIDs, this.includeUserState, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
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
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNHereNowResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNHereNowResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        

    }
}
