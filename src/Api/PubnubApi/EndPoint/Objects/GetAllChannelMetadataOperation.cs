using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
    public class GetAllChannelMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private int limit = -1;
        private bool includeCount;
        private bool includeCustom;
        private bool includeStatus;
        private bool includeType;
        private string channelsFilter;
        private PNPageObject page = new PNPageObject();
        private List<string> sortField;
        private PNCallback<PNGetAllChannelMetadataResult> savedCallback;
        private Dictionary<string, object> queryParam;

    public GetAllChannelMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
        IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig,
        jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
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

    public GetAllChannelMetadataOperation Page(PNPageObject pageObject)
    {
        page = pageObject;
        return this;
    }

    public GetAllChannelMetadataOperation Limit(int numberOfChannels)
    {
        limit = numberOfChannels;
        return this;
    }

    public GetAllChannelMetadataOperation IncludeCount(bool includeTotalCount)
    {
        includeCount = includeTotalCount;
        return this;
    }

    public GetAllChannelMetadataOperation IncludeCustom(bool includeCustomData)
    {
        includeCustom = includeCustomData;
        return this;
    }

    public GetAllChannelMetadataOperation IncludeStatus(bool includeStatusData)
    {
        includeStatus = includeStatusData;
        return this;
    }

    public GetAllChannelMetadataOperation IncludeType(bool includeTypeData)
    {
        includeType = includeTypeData;
        return this;
    }

    public GetAllChannelMetadataOperation Filter(string filterExpression)
    {
        channelsFilter = filterExpression;
        return this;
    }

    public GetAllChannelMetadataOperation Sort(List<string> sortByField)
    {
        sortField = sortByField;
        return this;
    }

    public GetAllChannelMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
    {
        queryParam = customQueryParam;
        return this;
    }

    public void Execute(PNCallback<PNGetAllChannelMetadataResult> callback)
    {
        savedCallback = callback;
        logger?.Trace($"{GetType().Name} Execute invoked");
        GetAllChannelMetadataList(savedCallback);
    }

    public async Task<PNResult<PNGetAllChannelMetadataResult>> ExecuteAsync()
    {
        logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
        return await GetAllChannelMetadataList().ConfigureAwait(false);
    }

    internal void Retry()
    {
        GetAllChannelMetadataList(savedCallback);
    }

    private void GetAllChannelMetadataList(PNCallback<PNGetAllChannelMetadataResult> callback)
    {
        if (callback == null)
        {
            throw new ArgumentException("Missing callback");
        }

        logger?.Debug($"{GetType().Name} parameter validated.");
        var requestState = new RequestState<PNGetAllChannelMetadataResult>
        {
            ResponseType = PNOperationType.PNGetAllChannelMetadataOperation,
            PubnubCallback = callback,
            Reconnect = false,
            EndPointOperation = this,
            UsePostMethod = false
        };
        var requestParameter = CreateRequestParameter();

        var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
            requestParameter, PNOperationType.PNGetAllChannelMetadataOperation);
        PubnubInstance.transportMiddleware.Send(transportRequest).ContinueWith(t =>
        {
            var transportResponse = t.Result;
            if (transportResponse.Error == null)
            {
                var responseString = Encoding.UTF8.GetString(transportResponse.Content);
                if (!string.IsNullOrEmpty(responseString))
                {
                    requestState.GotJsonResponse = true;
                    var result = ProcessJsonResponse(requestState, responseString);
                    ProcessResponseCallbacks(result, requestState);
                    logger?.Info(
                        $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                }
            }
            else
            {
                var statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
                var category =
                    PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
                var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                    PNOperationType.PNGetAllChannelMetadataOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                requestState.PubnubCallback.OnResponse(default, status);
                logger?.Info(
                    $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
            }
        });
    }

    private async Task<PNResult<PNGetAllChannelMetadataResult>> GetAllChannelMetadataList()
    {
        var returnValue = new PNResult<PNGetAllChannelMetadataResult>();
        var requestState = new RequestState<PNGetAllChannelMetadataResult>
        {
            ResponseType = PNOperationType.PNGetAllChannelMetadataOperation,
            Reconnect = false,
            EndPointOperation = this,
            UsePostMethod = false
        };
        Tuple<string, PNStatus> JsonAndStatusTuple;
        var requestParameter = CreateRequestParameter();
        var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
            requestParameter, PNOperationType.PNGetAllChannelMetadataOperation);
        var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest)
            .ConfigureAwait(false);
        if (transportResponse.Error == null)
        {
            var responseString = Encoding.UTF8.GetString(transportResponse.Content);
            var errorStatus = GetStatusIfError(requestState, responseString);
            if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode)
            {
                requestState.GotJsonResponse = true;
                var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                    requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState,
                    (int)HttpStatusCode.OK, null);
                JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
            }
            else
            {
                JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
            }

            returnValue.Status = JsonAndStatusTuple.Item2;
            var json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                var resultList = ProcessJsonResponse(requestState, json);
                var responseBuilder = new ResponseBuilder(config, jsonLibrary);
                var responseResult =
                    responseBuilder.JsonToObject<PNGetAllChannelMetadataResult>(resultList, true);
                if (responseResult != null)
                {
                    returnValue.Result = responseResult;
                }
            }
        }
        else
        {
            var statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
            var category =
                PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
            var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                PNOperationType.PNGetAllChannelMetadataOperation, category, requestState, statusCode,
                new PNException(transportResponse.Error.Message, transportResponse.Error));
            returnValue.Status = status;
        }

        logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
        return returnValue;
    }

    private RequestParameter CreateRequestParameter()
    {
        var pathSegments = new List<string>
        {
            "v2",
            "objects",
            config.SubscribeKey,
            "channels"
        };

        var requestQueryStringParams = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(page?.Next))
            requestQueryStringParams.Add("start",
                UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNGetAllChannelMetadataOperation, false,
                    false, false));

        if (!string.IsNullOrEmpty(page?.Prev))
            requestQueryStringParams.Add("end",
                UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNGetAllChannelMetadataOperation, false,
                    false, false));

        if (limit >= 0)
        {
            requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        }

        if (includeCount)
        {
            requestQueryStringParams.Add("count", "true");
        }

        var includes = new List<string>();
        if (includeCustom || includeStatus || includeType)
        {
            if (includeStatus)
            {
                includes.Add("status");
            }
            if (includeType)
            {
                includes.Add("type");
            }
            if (includeCustom)
            {
                includes.Add("custom");
            }
            var includeQueryString = string.Join(",", includes.ToArray());
            requestQueryStringParams.Add("include",
                UriUtil.EncodeUriComponent(includeQueryString, PNOperationType.PNGetAllChannelMetadataOperation,
                    false,
                    false, false));
        }

        if (!string.IsNullOrEmpty(channelsFilter))
        {
            requestQueryStringParams.Add("filter",
                UriUtil.EncodeUriComponent(channelsFilter, PNOperationType.PNGetAllChannelMetadataOperation, false,
                    false, false));
        }

        if (sortField != null && sortField.Count > 0)
        {
            requestQueryStringParams.Add("sort",
                UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()),
                    PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
        }

        if (queryParam != null && queryParam.Count > 0)
        {
            foreach (var kvp in queryParam)
            {
                if (!requestQueryStringParams.ContainsKey(kvp.Key))
                {
                    requestQueryStringParams.Add(kvp.Key,
                        UriUtil.EncodeUriComponent(kvp.Value.ToString(),
                            PNOperationType.PNGetAllChannelMetadataOperation, false, false, false));
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