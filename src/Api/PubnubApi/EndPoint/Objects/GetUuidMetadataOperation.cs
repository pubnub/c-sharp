using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint;

public class GetUuidMetadataOperation : PubnubCoreBase
{
    private readonly PNConfiguration config;
    private readonly IJsonPluggableLibrary jsonLibrary;
    private bool includeCustom;
    private bool includeStatus;
    private bool includeType;
    private Dictionary<string, object> queryParam;
    private PNCallback<PNGetUuidMetadataResult> savedCallback;
    private string uuid = string.Empty;

    public GetUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
        IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig,
        jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
    {
        config = pubnubConfig;
        jsonLibrary = jsonPluggableLibrary;


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

    public GetUuidMetadataOperation Uuid(string uuid)
    {
        this.uuid = uuid;
        return this;
    }

    public GetUuidMetadataOperation IncludeCustom(bool includeCustomData)
    {
        includeCustom = includeCustomData;
        return this;
    }

    public GetUuidMetadataOperation IncludeStatus(bool includeStatusData)
    {
        includeStatus = includeStatusData;
        return this;
    }

    public GetUuidMetadataOperation IncludeType(bool includeTypeData)
    {
        includeType = includeTypeData;
        return this;
    }

    public GetUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
    {
        queryParam = customQueryParam;
        return this;
    }

    public void Execute(PNCallback<PNGetUuidMetadataResult> callback)
    {
        savedCallback = callback;
        logger?.Trace($"{GetType().Name} Execute invoked");
        GetSingleUuidMetadata(savedCallback);
    }

    public async Task<PNResult<PNGetUuidMetadataResult>> ExecuteAsync()
    {
        logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
        return await GetSingleUuidMetadata().ConfigureAwait(false);
    }

    internal void Retry()
    {
        GetSingleUuidMetadata(savedCallback);
    }

    private void GetSingleUuidMetadata(PNCallback<PNGetUuidMetadataResult> callback)
    {
        if (callback == null) 
        {
            throw new ArgumentException("Missing callback");
        }

        if (string.IsNullOrEmpty(this.uuid)) 
        {
            this.uuid = config.UserId;
        }

        logger?.Debug($"{GetType().Name} parameter validated.");
        var requestState = new RequestState<PNGetUuidMetadataResult>
        {
            ResponseType = PNOperationType.PNGetUuidMetadataOperation,
            PubnubCallback = callback,
            UsePostMethod = false,
            Reconnect = false,
            EndPointOperation = this
        };

        var requestParameter = CreateRequestParameter();
        var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
            requestParameter, PNOperationType.PNGetUuidMetadataOperation);
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
                else
                {
                    var errorStatus = GetStatusIfError(requestState, responseString);
                    callback.OnResponse(null, errorStatus);
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
                    PNOperationType.PNGetUuidMetadataOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                requestState.PubnubCallback.OnResponse(null, status);
                logger?.Info(
                    $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
            }
        });
    }

    private async Task<PNResult<PNGetUuidMetadataResult>> GetSingleUuidMetadata()
    {
        if (string.IsNullOrEmpty(this.uuid)) { this.uuid = config.UserId;}

        logger?.Debug($"{GetType().Name} parameter validated.");
        var returnValue = new PNResult<PNGetUuidMetadataResult>();
        var requestState = new RequestState<PNGetUuidMetadataResult>
        {
            ResponseType = PNOperationType.PNGetUuidMetadataOperation,
            Reconnect = false,
            UsePostMethod = false,
            EndPointOperation = this
        };

        var requestParameter = CreateRequestParameter();
        var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
            requestParameter, PNOperationType.PNGetUuidMetadataOperation);
        var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest)
            .ConfigureAwait(false);
        if (transportResponse.Error == null)
        {
            var responseString = Encoding.UTF8.GetString(transportResponse.Content);
            var errorStatus = GetStatusIfError(requestState, responseString);
            Tuple<string, PNStatus> jsonAndStatusTuple;
            if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode)
            {
                requestState.GotJsonResponse = true;
                var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                    requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState,
                    (int)HttpStatusCode.OK, null);
                jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
            }
            else
            {
                jsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
            }

            returnValue.Status = jsonAndStatusTuple.Item2;
            var json = jsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                var resultList = ProcessJsonResponse(requestState, json);
                var responseBuilder = new ResponseBuilder(config, jsonLibrary);
                var responseResult =
                    responseBuilder.JsonToObject<PNGetUuidMetadataResult>(resultList, true);
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
                PNOperationType.PNGetUuidMetadataOperation, category, requestState, statusCode,
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
            "uuids",
            string.IsNullOrEmpty(uuid) ? string.Empty : uuid
        };

        var requestQueryStringParams = new Dictionary<string, string>();
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

        if (queryParam != null && queryParam.Count > 0)
        {
            foreach (var kvp in queryParam)
            {
                if (!requestQueryStringParams.ContainsKey(kvp.Key))
                {
                    requestQueryStringParams.Add(kvp.Key,
                        UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetUuidMetadataOperation,
                            false, false, false));
                }
            }
        }

        var requestParameter = new RequestParameter
        {
            RequestType = Constants.GET,
            PathSegment = pathSegments,
            Query = requestQueryStringParams
        };
        return requestParameter;
    }
}