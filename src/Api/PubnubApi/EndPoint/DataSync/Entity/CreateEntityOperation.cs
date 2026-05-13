using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class CreateEntityOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly CreateEntityParameters parameters;

        private PNCallback<PNDataSyncEntityResult> savedCallback;
        
        private const PNOperationType OperationType = PNOperationType.PNDataSyncCreateEntity;

        public CreateEntityOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance,
            CreateEntityParameters parameters) : base(pubnubConfig,
            jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public void Execute(PNCallback<PNDataSyncEntityResult> callback)
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

        public async Task<PNResult<PNDataSyncEntityResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await CreateEntityAsync().ConfigureAwait(false);
        }

        internal void Retry()
        {
            if (savedCallback != null)
            {
                Execute(savedCallback);
            }
        }

        private async Task<PNResult<PNDataSyncEntityResult>> CreateEntityAsync()
        {
            var returnValue = new PNResult<PNDataSyncEntityResult>();

            if (string.IsNullOrEmpty(parameters.EntityClass) || string.IsNullOrEmpty(parameters.EntityClass.Trim()))
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing EntityClass",
                        new ArgumentException("Missing EntityClass"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            if (parameters.EntityClassVersion < 1)
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("EntityClassVersion must be >= 1",
                        new ArgumentException("EntityClassVersion must be >= 1"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            if (string.IsNullOrEmpty(parameters.IdempotencyKey) || string.IsNullOrEmpty(parameters.IdempotencyKey.Trim()))
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing IdempotencyKey",
                        new ArgumentException("Missing IdempotencyKey"))
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
            var requestState = new RequestState<PNDataSyncEntityResult>
            {
                ResponseType = OperationType,
                Reconnect = false,
                EndPointOperation = this,
                UsePostMethod = true
            };

            var requestParameter = CreateRequestParameter();
            Tuple<string, PNStatus> JsonAndStatusTuple;
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter, OperationType);
            var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest)
                .ConfigureAwait(false);
            if (transportResponse.Error == null)
            {
                var responseString = Encoding.UTF8.GetString(transportResponse.Content);
                var errorStatus = GetStatusIfError(requestState, responseString);
                //TODO: why 201 and not 200 from constants?
                if (errorStatus == null && transportResponse.StatusCode == 201)
                {
                    requestState.GotJsonResponse = true;
                    var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                        requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 201,
                        null);
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
                        responseBuilder.JsonToObject<PNDataSyncEntityResult>(resultList, true);
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
            var dataProperties = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(parameters.Id))
            {
                dataProperties.Add("id", parameters.Id);
            }

            dataProperties.Add("entityClass", parameters.EntityClass);
            dataProperties.Add("entityClassVersion", parameters.EntityClassVersion);

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                dataProperties.Add("status", parameters.Status);
            }

            if (parameters.Payload != null)
            {
                dataProperties.Add("payload", parameters.Payload);
            }

            var requestEnvelope = new Dictionary<string, object>
            {
                { "data", dataProperties }
            };

            var postBody = jsonLibrary.SerializeToJsonString(requestEnvelope);

            var pathSegments = new List<string>
            {
                //"v1",
                //"datasync",
                "subkeys",
                config.SubscribeKey,
                "entities"
            };

            var requestParameter = new RequestParameter
            {
                RequestType = Constants.POST,
                PathSegment = pathSegments,
                BodyContentString = postBody
            };

            requestParameter.Headers.Add("Content-Type",
                "application/vnd.pubnub.objects.entity+json;version=1");
            requestParameter.Headers.Add("Idempotency-Key", parameters.IdempotencyKey);

            return requestParameter;
        }
    }
}
