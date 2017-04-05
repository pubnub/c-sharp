using System;
using PubnubApi.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class DeleteChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private string channelGroupName = "";
        private PNCallback<PNChannelGroupsDeleteGroupResult> savedCallback = null;

        public DeleteChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public DeleteChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public DeleteChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public DeleteChannelGroupOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public void Async(PNCallback<PNChannelGroupsDeleteGroupResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                DeleteChannelGroup(this.channelGroupName, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                DeleteChannelGroup(this.channelGroupName, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void DeleteChannelGroup(string groupName, PNCallback<PNChannelGroupsDeleteGroupResult> callback)
        {
            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);

            Uri request = urlBuilder.BuildRemoveChannelsFromChannelGroupRequest(null, "", groupName);

            RequestState<PNChannelGroupsDeleteGroupResult> requestState = new RequestState<PNChannelGroupsDeleteGroupResult>();
            requestState.ResponseType = PNOperationType.PNRemoveGroupOperation;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { groupName };
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNChannelGroupsDeleteGroupResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsDeleteGroupResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
