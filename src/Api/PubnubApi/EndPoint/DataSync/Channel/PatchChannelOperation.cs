using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class PatchChannelOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly PatchChannelParameters parameters;

        private PNCallback<PNDataSyncChannelResult> savedCallback;

        private const PNOperationType OperationType = PNOperationType.PNDataSyncPatchChannel;

        private static readonly HashSet<JsonPatchOperationType> OpsRequiringValue =
            new () { JsonPatchOperationType.Add, JsonPatchOperationType.Replace, JsonPatchOperationType.Test };

        private static readonly HashSet<JsonPatchOperationType> OpsRequiringFrom =
            new () { JsonPatchOperationType.Move, JsonPatchOperationType.Copy };

        public PatchChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance,
            PatchChannelParameters parameters) : base(pubnubConfig,
            jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public void Execute(PNCallback<PNDataSyncChannelResult> callback)
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

        public async Task<PNResult<PNDataSyncChannelResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await PatchChannelAsync().ConfigureAwait(false);
        }

        internal void Retry()
        {
            if (savedCallback != null)
            {
                Execute(savedCallback);
            }
        }

        private async Task<PNResult<PNDataSyncChannelResult>> PatchChannelAsync()
        {
            var returnValue = new PNResult<PNDataSyncChannelResult>();

            if (string.IsNullOrEmpty(parameters.Id) || string.IsNullOrEmpty(parameters.Id.Trim()))
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing Channel Id",
                        new ArgumentException("Missing Channel Id"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            if (parameters.Operations == null || parameters.Operations.Count == 0)
            {
                var errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Operations list must contain at least one operation",
                        new ArgumentException("Operations list must contain at least one operation"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            if (string.IsNullOrEmpty(parameters.IdempotencyKey) ||
                string.IsNullOrEmpty(parameters.IdempotencyKey.Trim()))
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
            var requestState = new RequestState<PNDataSyncChannelResult>
            {
                ResponseType = OperationType,
                Reconnect = false,
                EndPointOperation = this,
                UsePostMethod = false
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
                        responseBuilder.JsonToObject<PNDataSyncChannelResult>(resultList, true);
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
            var patchArray = new List<Dictionary<string, object>>();

            foreach (var op in parameters.Operations)
            {
                var entry = new Dictionary<string, object>
                {
                    { "op", op.Op.ToString().ToLowerInvariant() },
                    { "path", op.Path }
                };

                if (OpsRequiringValue.Contains(op.Op))
                {
                    entry["value"] = op.Value;
                }

                if (OpsRequiringFrom.Contains(op.Op) && !string.IsNullOrEmpty(op.From))
                {
                    entry["from"] = op.From;
                }

                patchArray.Add(entry);
            }

            var patchBody = jsonLibrary.SerializeToJsonString(patchArray);

             var pathSegments = new List<string>
            {
                "v1",
                "datasync",
                "subkeys",
                config.SubscribeKey,
                "channels",
                parameters.Id
            };

            var requestParameter = new RequestParameter
            {
                RequestType = Constants.PATCH,
                PathSegment = pathSegments,
                BodyContentString = patchBody
            };

            requestParameter.Headers.Add("Content-Type", "application/json-patch+json");
            requestParameter.Headers.Add("Idempotency-Key", parameters.IdempotencyKey);

            if (!string.IsNullOrEmpty(parameters.IfMatch))
            {
                requestParameter.Headers.Add("If-Match", parameters.IfMatch);
            }

            return requestParameter;
        }
    }
}
