using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class GetRelationshipsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly GetRelationshipsParameters parameters;

        private PNCallback<PNDataSyncRelationshipsListResult> savedCallback;

        private const PNOperationType OperationType = PNOperationType.PNDataSyncGetRelationships;

        public GetRelationshipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance,
            GetRelationshipsParameters parameters) : base(pubnubConfig,
            jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public void Execute(PNCallback<PNDataSyncRelationshipsListResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

            savedCallback = callback;
            ExecuteAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var status = new PNStatus
                    {
                        Error = true,
                        ErrorData = new PNErrorData(t.Exception?.Message, t.Exception)
                    };
                    callback.OnResponse(default, status);
                }
                else
                {
                    var pnResult = t.Result;
                    callback.OnResponse(pnResult.Result, pnResult.Status);
                }
            });
        }

        public async Task<PNResult<PNDataSyncRelationshipsListResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await GetRelationshipsAsync().ConfigureAwait(false);
        }

        internal void Retry()
        {
            if (savedCallback != null)
            {
                Execute(savedCallback);
            }
        }

        private async Task<PNResult<PNDataSyncRelationshipsListResult>> GetRelationshipsAsync()
        {
            var returnValue = new PNResult<PNDataSyncRelationshipsListResult>();

            if (string.IsNullOrEmpty(parameters.RelationshipClass) || string.IsNullOrEmpty(parameters.RelationshipClass.Trim()))
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing RelationshipClass",
                        new ArgumentException("Missing RelationshipClass"))
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
                    ErrorData = new PNErrorData("Invalid Subscribe key",
                        new ArgumentException("Invalid Subscribe key"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            logger?.Trace($"{GetType().Name} parameter validated.");
            var requestState = new RequestState<PNDataSyncRelationshipsListResult>
            {
                ResponseType = OperationType,
                Reconnect = false,
                EndPointOperation = this,
                UsePostMethod = false
            };

            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter, OperationType);
            var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest)
                .ConfigureAwait(false);
            if (transportResponse.Error == null)
            {
                var responseString = Encoding.UTF8.GetString(transportResponse.Content);
                var errorStatus = GetStatusIfError(requestState, responseString);
                Tuple<string, PNStatus> JsonAndStatusTuple;
                if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode)
                {
                    requestState.GotJsonResponse = true;
                    var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                        requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState,
                        Constants.HttpRequestSuccessStatusCode, null);
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
                        responseBuilder.JsonToObject<PNDataSyncRelationshipsListResult>(resultList, true);
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
                    OperationType, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                returnValue.Status = status;
            }

            logger?.Trace($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
             var pathSegments = new List<string>
            {
                "v1",
                "datasync",
                "subkeys",
                config.SubscribeKey,
                "relationships"
            };

            var requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("relationship_class",
                UriUtil.EncodeUriComponent(parameters.RelationshipClass,
                    OperationType, false, false, false));

            if (!string.IsNullOrEmpty(parameters.EntityAId))
            {
                requestQueryStringParams.Add("entity_a_id",
                    UriUtil.EncodeUriComponent(parameters.EntityAId,
                        OperationType, false, false, false));
            }

            if (!string.IsNullOrEmpty(parameters.EntityBId))
            {
                requestQueryStringParams.Add("entity_b_id",
                    UriUtil.EncodeUriComponent(parameters.EntityBId,
                        OperationType, false, false, false));
            }

            if (!string.IsNullOrEmpty(parameters.Cursor))
            {
                requestQueryStringParams.Add("cursor",
                    UriUtil.EncodeUriComponent(parameters.Cursor,
                        OperationType, false, false, false));
            }

            if (parameters.Limit.HasValue)
            {
                requestQueryStringParams.Add("limit",
                    parameters.Limit.Value.ToString());
            }

            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                requestQueryStringParams.Add("filter",
                    UriUtil.EncodeUriComponent(parameters.Filter,
                        OperationType, false, false, false));
            }

            if (!string.IsNullOrEmpty(parameters.FilterAdvanced))
            {
                requestQueryStringParams.Add("filter_advanced",
                    UriUtil.EncodeUriComponent(parameters.FilterAdvanced,
                        OperationType, false, false, false));
            }

            if (!string.IsNullOrEmpty(parameters.Sort))
            {
                requestQueryStringParams.Add("sort",
                    UriUtil.EncodeUriComponent(parameters.Sort,
                        OperationType, false, false, false));
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
}
