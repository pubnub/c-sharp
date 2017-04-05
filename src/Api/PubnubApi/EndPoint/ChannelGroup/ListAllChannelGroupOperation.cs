using System;
using PubnubApi.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class ListAllChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private PNCallback<PNChannelGroupsListAllResult> savedCallback = null;

        public ListAllChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public ListAllChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public ListAllChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public void Async(PNCallback<PNChannelGroupsListAllResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetAllChannelGroup(callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                GetAllChannelGroup(savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void GetAllChannelGroup(PNCallback<PNChannelGroupsListAllResult> callback)
        {
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);

            Uri request = urlBuilder.BuildGetAllChannelGroupRequest();

            RequestState<PNChannelGroupsListAllResult> requestState = new RequestState<PNChannelGroupsListAllResult>();
            requestState.ResponseType = PNOperationType.ChannelGroupAllGet;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNChannelGroupsListAllResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsListAllResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
