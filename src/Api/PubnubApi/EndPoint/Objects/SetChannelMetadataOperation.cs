using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class SetChannelMetadataOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private string channelDescription;


        private string channelId = string.Empty;
        private string channelName;
        private string channelStatus;
        private string channelType;
        private Dictionary<string, object> custom;

        private string ifMatchesEtag;
        private bool includeCustom = true;
        private bool includeStatus = true;
        private bool includeType = true;
        private Dictionary<string, object> queryParam;
        private PNCallback<PNSetChannelMetadataResult> savedCallback;

        public SetChannelMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig,
            jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public SetChannelMetadataOperation Channel(string channelName)
        {
            channelId = channelName;
            return this;
        }

        public SetChannelMetadataOperation Name(string channelMetadataName)
        {
            channelName = channelMetadataName;
            return this;
        }

        public SetChannelMetadataOperation Description(string channelMetadataDescription)
        {
            channelDescription = channelMetadataDescription;
            return this;
        }

        public SetChannelMetadataOperation Status(string status)
        {
            channelStatus = status;
            return this;
        }

        public SetChannelMetadataOperation Type(string type)
        {
            channelType = type;
            return this;
        }

        public SetChannelMetadataOperation Custom(Dictionary<string, object> channelMetadataCustomObject)
        {
            custom = channelMetadataCustomObject;
            return this;
        }

        public SetChannelMetadataOperation IncludeCustom(bool includeCustomData)
        {
            includeCustom = includeCustomData;
            return this;
        }

        public SetChannelMetadataOperation IncludeStatus(bool shouldIncludeStatusData)
        {
            includeStatus = shouldIncludeStatusData;
            return this;
        }

        public SetChannelMetadataOperation IncludeType(bool shouldIncludeTypeData)
        {
            includeType = shouldIncludeTypeData;
            return this;
        }

        public SetChannelMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public SetChannelMetadataOperation IfMatchesEtag(string etag)
        {
            ifMatchesEtag = etag;
            return this;
        }

        public void Execute(PNCallback<PNSetChannelMetadataResult> callback)
        {
            logger?.Trace($"{GetType().Name} Execute invoked");
            if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(channelId.Trim()))
            {
                throw new ArgumentException("Missing Channel");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) ||
                config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

            savedCallback = callback;
            logger?.Debug($"{GetType().Name} parameter validated.");
            SetChannelMetadata(callback);
        }

        public async Task<PNResult<PNSetChannelMetadataResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await SetChannelMetadata().ConfigureAwait(false);
        }

        internal void Retry()
        {
            SetChannelMetadata(savedCallback);
        }

        private void SetChannelMetadata(PNCallback<PNSetChannelMetadataResult> callback)
        {
            var requestState = new RequestState<PNSetChannelMetadataResult>
            {
                ResponseType = PNOperationType.PNSetChannelMetadataOperation,
                PubnubCallback = callback,
                Reconnect = false,
                EndPointOperation = this,
                UsePatchMethod = true
            };

            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter, PNOperationType.PNSetChannelMetadataOperation);
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
                        PNOperationType.PNSetChannelMetadataOperation, category, requestState, statusCode,
                        new PNException(transportResponse.Error.Message, transportResponse.Error));
                    requestState.PubnubCallback.OnResponse(default, status);
                    logger?.Info(
                        $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                }
            });
        }

        private async Task<PNResult<PNSetChannelMetadataResult>> SetChannelMetadata()
        {
            var returnValue = new PNResult<PNSetChannelMetadataResult>();

            if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(channelId.Trim()))
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) ||
                config.SubscribeKey.Length <= 0)
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            var requestState = new RequestState<PNSetChannelMetadataResult>
            {
                ResponseType = PNOperationType.PNSetChannelMetadataOperation,
                Reconnect = false,
                EndPointOperation = this,
                UsePatchMethod = true
            };

            var requestParameter = CreateRequestParameter();
            Tuple<string, PNStatus> JsonAndStatusTuple;
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter, PNOperationType.PNSetChannelMetadataOperation);
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
                        requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 200, null);
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
                        responseBuilder.JsonToObject<PNSetChannelMetadataResult>(resultList, true);
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
                    PNOperationType.PNSetChannelMetadataOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                returnValue.Status = status;
            }

            logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
            var messageEnvelope = new Dictionary<string, object>();
            if (channelName != null)
            {
                messageEnvelope.Add("name", channelName);
            }

            if (channelDescription != null)
            {
                messageEnvelope.Add("description", channelDescription);
            }

            if (custom != null)
            {
                messageEnvelope.Add("custom", custom);
            }

            if (channelStatus != null && !string.IsNullOrEmpty(channelStatus))
            {
                messageEnvelope.Add("status", channelStatus);
            }

            if (channelType != null && !string.IsNullOrEmpty(channelType))
            {
                messageEnvelope.Add("type", channelType);
            }

            var patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            var pathSegments = new List<string>
            {
                "v2",
                "objects",
                config.SubscribeKey,
                "channels",
                string.IsNullOrEmpty(channelId) ? string.Empty : channelId
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
                    UriUtil.EncodeUriComponent(includeQueryString, PNOperationType.PNSetChannelMetadataOperation, false,
                        false, false));
            }

            if (queryParam is { Count: > 0 })
            {
                foreach (var kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key,
                            UriUtil.EncodeUriComponent(kvp.Value.ToString(),
                                PNOperationType.PNSetChannelMetadataOperation, false, false, false));
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
            if (!string.IsNullOrEmpty(ifMatchesEtag)) requestParameter.Headers.Add("If-Match", ifMatchesEtag);

            return requestParameter;
        }
    }
}