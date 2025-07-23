using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class SetUuidMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private string ifMatchesEtag;
        private bool includeCustom = true;
        private bool includeStatus = true;
        private bool includeType = true;
        private Dictionary<string, object> queryParam;

        private PNCallback<PNSetUuidMetadataResult> savedCallback;
        private Dictionary<string, object> uuidCustom;
        private string uuidEmail;
        private string uuidExternalId;

        private string uuidId;
        private string uuidName;
        private string uuidProfileUrl;
        private string uuidStatus;
        private string uuidType;

        public SetUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
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

        public SetUuidMetadataOperation Uuid(string uuid)
        {
            uuidId = uuid;
            return this;
        }

        public SetUuidMetadataOperation Name(string name)
        {
            uuidName = name;
            return this;
        }

        public SetUuidMetadataOperation Email(string email)
        {
            uuidEmail = email;
            return this;
        }


        public SetUuidMetadataOperation ExternalId(string externalId)
        {
            uuidExternalId = externalId;
            return this;
        }


        public SetUuidMetadataOperation ProfileUrl(string profileUrl)
        {
            uuidProfileUrl = profileUrl;
            return this;
        }

        public SetUuidMetadataOperation Custom(Dictionary<string, object> customObject)
        {
            uuidCustom = customObject;
            return this;
        }

        public SetUuidMetadataOperation Status(string status)
        {
            uuidStatus = status;
            return this;
        }

        public SetUuidMetadataOperation Type(string type)
        {
            uuidType = type;
            return this;
        }

        public SetUuidMetadataOperation IncludeCustom(bool includeCustomData)
        {
            includeCustom = includeCustomData;
            return this;
        }

        public SetUuidMetadataOperation IncludeStatus(bool shouldIncludeStatusData)
        {
            includeStatus = shouldIncludeStatusData;
            return this;
        }

        public SetUuidMetadataOperation IncludeType(bool shouldIncludeTypeData)
        {
            includeType = shouldIncludeTypeData;
            return this;
        }

        public SetUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public SetUuidMetadataOperation IfMatchesEtag(string etag)
        {
            ifMatchesEtag = etag;
            return this;
        }

        public void Execute(PNCallback<PNSetUuidMetadataResult> callback)
        {
            logger?.Trace($"{GetType().Name} Execute invoked");
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

            savedCallback = callback;
            SetUuidMetadata(savedCallback);
        }

        public async Task<PNResult<PNSetUuidMetadataResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await SetUuidMetadata().ConfigureAwait(false);
        }

        internal void Retry()
        {
            SetUuidMetadata(savedCallback);
        }

        private void SetUuidMetadata(PNCallback<PNSetUuidMetadataResult> callback)
        {
            if (string.IsNullOrEmpty(uuidId))
            {
                uuidId = config.UserId;
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            var requestState = new RequestState<PNSetUuidMetadataResult>
            {
                ResponseType = PNOperationType.PNSetUuidMetadataOperation,
                PubnubCallback = callback,
                Reconnect = false,
                EndPointOperation = this,
                UsePatchMethod = true
            };
            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter, PNOperationType.PNSetUuidMetadataOperation);
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
                        callback.OnResponse(default, errorStatus);
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
                        PNOperationType.PNSetUuidMetadataOperation, category, requestState, statusCode,
                        new PNException(transportResponse.Error.Message, transportResponse.Error));
                    requestState.PubnubCallback.OnResponse(default, status);
                    logger?.Info(
                        $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                }
            });
        }

        private async Task<PNResult<PNSetUuidMetadataResult>> SetUuidMetadata()
        {
            if (string.IsNullOrEmpty(uuidId))
            {
                uuidId = config.UserId;
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            var returnValue = new PNResult<PNSetUuidMetadataResult>();

            var requestState = new RequestState<PNSetUuidMetadataResult>
            {
                ResponseType = PNOperationType.PNSetUuidMetadataOperation,
                Reconnect = false,
                EndPointOperation = this,
                UsePatchMethod = true
            };
            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter, PNOperationType.PNSetUuidMetadataOperation);
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
                        responseBuilder.JsonToObject<PNSetUuidMetadataResult>(resultList, true);
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
                    PNOperationType.PNSetUuidMetadataOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                returnValue.Status = status;
            }

            logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
            var messageEnvelope = new Dictionary<string, object>();
            if (uuidName != null)
            {
                messageEnvelope.Add("name", uuidName);
            }

            if (uuidExternalId != null)
            {
                messageEnvelope.Add("externalId", uuidExternalId);
            }

            if (uuidProfileUrl != null)
            {
                messageEnvelope.Add("profileUrl", uuidProfileUrl);
            }

            if (uuidEmail != null)
            {
                messageEnvelope.Add("email", uuidEmail);
            }

            if (uuidCustom != null)
            {
                messageEnvelope.Add("custom", uuidCustom);
            }

            if (uuidStatus != null && !string.IsNullOrEmpty(uuidStatus))
            {
                messageEnvelope.Add("status", uuidStatus);
            }

            if (uuidType != null && !string.IsNullOrEmpty(uuidType))
            {
                messageEnvelope.Add("type", uuidType);
            }

            var patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            var pathSegments = new List<string>
            {
                "v2",
                "objects",
                config.SubscribeKey,
                "uuids",
                string.IsNullOrEmpty(uuidId) ? string.Empty : uuidId
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
                    UriUtil.EncodeUriComponent(includeQueryString, PNOperationType.PNSetUuidMetadataOperation, false,
                        false, false));
            }

            if (queryParam is { Count: > 0 })
            {
                foreach (var kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key,
                            UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNSetUuidMetadataOperation,
                                false, false, false));
                    }
                }
            }

            var requestParameter = new RequestParameter
            {
                RequestType = Constants.PATCH,
                PathSegment = pathSegments,
                Query = requestQueryStringParams,
                BodyContentString = patchMessage
            };
            if (!string.IsNullOrEmpty(ifMatchesEtag))
            {
                requestParameter.Headers.Add("If-Match", ifMatchesEtag);
            }

            return requestParameter;
        }
    }
}