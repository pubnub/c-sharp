﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
    public class SetChannelMembersOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private string channelId = string.Empty;
        private List<PNChannelMember> setMember;
        private PNPageObject page = new PNPageObject();
        private int limit = -1;
        private bool includeCount;

        private PNChannelMemberField[] includeFields =
            [PNChannelMemberField.CUSTOM, PNChannelMemberField.STATUS, PNChannelMemberField.TYPE];

        private List<string> sortField;
        private PNCallback<PNChannelMembersResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SetChannelMembersOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig,
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

        public SetChannelMembersOperation Channel(string channelId)
        {
            this.channelId = channelId;
            return this;
        }

        public SetChannelMembersOperation Uuids(List<PNChannelMember> channelMembers)
        {
            this.setMember = channelMembers;
            return this;
        }

        public SetChannelMembersOperation Include(PNChannelMemberField[] includeOptions)
        {
            includeFields = includeOptions;
            return this;
        }

        public SetChannelMembersOperation Page(PNPageObject pageObject)
        {
            this.page = pageObject;
            return this;
        }

        public SetChannelMembersOperation Limit(int numberOfObjects)
        {
            this.limit = numberOfObjects;
            return this;
        }

        public SetChannelMembersOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public SetChannelMembersOperation Sort(List<string> sortByField)
        {
            this.sortField = sortByField;
            return this;
        }

        public SetChannelMembersOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNChannelMembersResult> callback)
        {
            logger?.Trace($"{GetType().Name} Execute invoked");
            if (string.IsNullOrEmpty(this.channelId) || string.IsNullOrEmpty(channelId.Trim()))
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
                throw new ArgumentException("Missing callback");
            }

            this.savedCallback = callback;
            ProcessMembersOperationRequest(callback);
        }

        public async Task<PNResult<PNChannelMembersResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await ProcessMembersOperationRequest(this.channelId)
                .ConfigureAwait(false);
        }

        internal void Retry()
        {
            ProcessMembersOperationRequest(savedCallback);
        }

        private void ProcessMembersOperationRequest(PNCallback<PNChannelMembersResult> callback)
        {
            logger?.Debug($"{GetType().Name} parameter validated.");
            RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>
            {
                ResponseType = PNOperationType.PNSetChannelMembersOperation,
                PubnubCallback = callback,
                Reconnect = false,
                UsePatchMethod = true,
                EndPointOperation = this
            };
            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter: requestParameter, operationType: PNOperationType.PNSetChannelMembersOperation);
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
                        callback.OnResponse(default, errorStatus);
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
                        PNOperationType.PNSetChannelMembersOperation, category, requestState, statusCode,
                        new PNException(transportResponse.Error.Message, transportResponse.Error));
                    requestState.PubnubCallback.OnResponse(default, status);
                    logger?.Info(
                        $"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
                }
            });
        }

        private async Task<PNResult<PNChannelMembersResult>> ProcessMembersOperationRequest(string channel)
        {
            PNResult<PNChannelMembersResult> returnValue = new PNResult<PNChannelMembersResult>();

            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()))
            {
                PNStatus errStatus = new PNStatus
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
                PNStatus errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            RequestState<PNChannelMembersResult> requestState = new RequestState<PNChannelMembersResult>
            {
                ResponseType = PNOperationType.PNSetChannelMembersOperation,
                Reconnect = false,
                UsePatchMethod = true,
                EndPointOperation = this
            };
            var requestParameter = CreateRequestParameter();
            Tuple<string, PNStatus> JsonAndStatusTuple;
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter: requestParameter, operationType: PNOperationType.PNSetChannelMembersOperation);
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
                    PNChannelMembersResult responseResult =
                        responseBuilder.JsonToObject<PNChannelMembersResult>(resultList, true);
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
                    PNOperationType.PNSetChannelMembersOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                returnValue.Status = status;
            }

            logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (setMember != null)
            {
                List<Dictionary<string, object>> setMemberFormatList = new List<Dictionary<string, object>>();
                foreach (var member in setMember)
                {
                    Dictionary<string, object> currentMemberFormat = new Dictionary<string, object>
                    {
                        { "uuid", new Dictionary<string, string> { { "id", member.Uuid } } }
                    };
                    if (member.Custom != null)
                    {
                        currentMemberFormat.Add("custom", member.Custom);
                    }

                    if (member.Status != null)
                    {
                        currentMemberFormat.Add("status", member.Status);
                    }

                    if (member.Type != null)
                    {
                        currentMemberFormat.Add("type", member.Type);
                    }

                    setMemberFormatList.Add(currentMemberFormat);
                }

                if (setMemberFormatList.Count > 0)
                {
                    messageEnvelope.Add("set", setMemberFormatList);
                }
            }

            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            List<string> pathSegments = new List<string>
            {
                "v2",
                "objects",
                config.SubscribeKey,
                "channels",
                string.IsNullOrEmpty(channelId) ? string.Empty : channelId,
                "uuids"
            };

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(page.Next))
            {
                requestQueryStringParams.Add("start",
                    UriUtil.EncodeUriComponent(page.Next, PNOperationType.PNSetChannelMembersOperation, false, false,
                        false));
            }

            if (!string.IsNullOrEmpty(page.Prev))
            {
                requestQueryStringParams.Add("end",
                    UriUtil.EncodeUriComponent(page.Prev, PNOperationType.PNSetChannelMembersOperation, false, false,
                        false));
            }

            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }

            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }

            if (includeFields != null)
            {
                string[] arrayInclude = includeFields
                    .Select(x => UrlParameterConverter.MapEnumValueToEndpoint(x.ToString())).ToArray();
                var commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
                requestQueryStringParams.Add("include",
                    UriUtil.EncodeUriComponent(commandDelimitedIncludeOptions,
                        PNOperationType.PNSetChannelMembersOperation, false, false, false));
            }

            if (sortField != null && sortField.Count > 0)
            {
                requestQueryStringParams.Add("sort",
                    UriUtil.EncodeUriComponent(string.Join(",", sortField.ToArray()),
                        PNOperationType.PNSetChannelMembersOperation, false, false, false));
            }

            if (queryParam != null && queryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key,
                            UriUtil.EncodeUriComponent(kvp.Value.ToString(),
                                PNOperationType.PNSetChannelMembersOperation, false, false, false));
                    }
                }
            }

            var requestParameter = new RequestParameter()
            {
                RequestType = Constants.PATCH,
                PathSegment = pathSegments,
                Query = requestQueryStringParams,
                BodyContentString = patchMessage
            };
            return requestParameter;
        }
    }
}