using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
    public class GetChannelMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private string channelId = "";
        private bool includeCustom;

        private PNCallback<PNGetChannelMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GetChannelMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(
            pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;

            if (instance != null)
            {
                if (!ChannelRequest.ContainsKey(instance.InstanceId))
                {
                    ChannelRequest.GetOrAdd(instance.InstanceId,
                        new ConcurrentDictionary<string, CancellationTokenSource>());
                }

                if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
                {
                    ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
                }

                if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
                {
                    ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
                }
            }
        }

        public GetChannelMetadataOperation Channel(string channelName)
        {
            this.channelId = channelName;
            return this;
        }

        public GetChannelMetadataOperation IncludeCustom(bool includeCustomData)
        {
            this.includeCustom = includeCustomData;
            return this;
        }

        public GetChannelMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNGetChannelMetadataResult> callback)
        {
            logger?.Trace($"{GetType().Name} Execute invoked");
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

            if (string.IsNullOrEmpty(this.channelId))
            {
                throw new ArgumentException("Missing Channel");
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            this.savedCallback = callback;
            GetSingleChannelMetadata(this.channelId, this.includeCustom, this.queryParam, savedCallback);
        }

        public async Task<PNResult<PNGetChannelMetadataResult>> ExecuteAsync()
        {
            return await GetSingleChannelMetadata(this.channelId, this.includeCustom, this.queryParam)
                .ConfigureAwait(false);
        }

        internal void Retry()
        {
            GetSingleChannelMetadata(this.channelId, this.includeCustom, this.queryParam, savedCallback);
        }

        private void GetSingleChannelMetadata(string spaceId, bool includeCustom,
            Dictionary<string, object> externalQueryParam, PNCallback<PNGetChannelMetadataResult> callback)
        {
            RequestState<PNGetChannelMetadataResult> requestState = new RequestState<PNGetChannelMetadataResult>
            {
                ResponseType = PNOperationType.PNGetChannelMetadataOperation,
                PubnubCallback = callback,
                Reconnect = false,
                UsePostMethod = false,
                EndPointOperation = this
            };

            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter: requestParameter, operationType: PNOperationType.PNGetChannelMetadataOperation);
            PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t =>
            {
                var transportResponse = t.Result;
                if (transportResponse.Error == null)
                {
                    var responseString = Encoding.UTF8.GetString(transportResponse.Content);
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        requestState.GotJsonResponse = true;
                        List<object> result = ProcessJsonResponse(requestState, responseString);
                        ProcessResponseCallbacks(result, requestState);
                        logger?.Info(
                            $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                    }
                    else
                    {
                        PNStatus errorStatus = GetStatusIfError(requestState, responseString);
                        callback.OnResponse(null, errorStatus);
                        logger?.Info(
                            $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                    }
                }
                else
                {
                    int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
                    PNStatusCategory category =
                        PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                        PNOperationType.PNGetChannelMetadataOperation, category, requestState, statusCode,
                        new PNException(transportResponse.Error.Message, transportResponse.Error));
                    requestState.PubnubCallback.OnResponse(default, status);
                    logger?.Info(
                        $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                }
            });
        }

        private async Task<PNResult<PNGetChannelMetadataResult>> GetSingleChannelMetadata(string spaceId,
            bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNGetChannelMetadataResult> returnValue = new PNResult<PNGetChannelMetadataResult>();

            if (string.IsNullOrEmpty(this.channelId))
            {
                PNStatus errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            RequestState<PNGetChannelMetadataResult> requestState = new RequestState<PNGetChannelMetadataResult>
            {
                ResponseType = PNOperationType.PNGetChannelMetadataOperation,
                Reconnect = false,
                UsePostMethod = false,
                EndPointOperation = this
            };
            var requestParameter = CreateRequestParameter();
            Tuple<string, PNStatus> JsonAndStatusTuple;
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter: requestParameter, operationType: PNOperationType.PNGetChannelMetadataOperation);
            var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest)
                .ConfigureAwait(false);
            if (transportResponse.Error == null)
            {
                var responseString = Encoding.UTF8.GetString(transportResponse.Content);
                PNStatus errorStatus = GetStatusIfError(requestState, responseString);
                if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode)
                {
                    requestState.GotJsonResponse = true;
                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                        requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState,
                        (int)HttpStatusCode.OK, null);
                    JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
                }
                else
                {
                    JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
                }

                returnValue.Status = JsonAndStatusTuple.Item2;
                string json = JsonAndStatusTuple.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> resultList = ProcessJsonResponse(requestState, json);
                    ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
                    PNGetChannelMetadataResult responseResult =
                        responseBuilder.JsonToObject<PNGetChannelMetadataResult>(resultList, true);
                    if (responseResult != null)
                    {
                        returnValue.Result = responseResult;
                    }
                }
            }
            else
            {
                int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
                PNStatusCategory category =
                    PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                    PNOperationType.PNGetChannelMetadataOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                returnValue.Status = status;
            }

            logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
            List<string> pathSegments = new List<string>
            {
                "v2",
                "objects",
                config.SubscribeKey,
                "channels",
                string.IsNullOrEmpty(channelId) ? string.Empty : channelId
            };

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }

            if (queryParam != null && queryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key,
                            UriUtil.EncodeUriComponent(kvp.Value.ToString(),
                                PNOperationType.PNGetChannelMetadataOperation, false, false, false));
                    }
                }
            }

            var requestParameter = new RequestParameter()
            {
                RequestType = Constants.GET,
                PathSegment = pathSegments,
                Query = requestQueryStringParams
            };
            return requestParameter;
        }
    }
}