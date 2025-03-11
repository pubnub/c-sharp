using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class RemoveUuidMetadataOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string uuid = string.Empty;

		private PNCallback<PNRemoveUuidMetadataResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public RemoveUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;

			if (instance != null) {
				if (!ChannelRequest.ContainsKey(instance.InstanceId)) {
					ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
				}
				if (!ChannelInternetStatus.ContainsKey(instance.InstanceId)) {
					ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
				}
				if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId)) {
					ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
				}
			}
		}

		public RemoveUuidMetadataOperation Uuid(string uuid)
		{
			this.uuid = uuid;
			return this;
		}

		public RemoveUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNRemoveUuidMetadataResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				throw new MissingMemberException("Invalid Subscribe key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}

			this.savedCallback = callback;
			RemoveUuidMetadata(this.uuid, this.queryParam, callback);
		}


		public async Task<PNResult<PNRemoveUuidMetadataResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await RemoveUuidMetadata(this.uuid, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			RemoveUuidMetadata(this.uuid, this.queryParam, savedCallback);
		}

		private void RemoveUuidMetadata(string uuid, Dictionary<string, object> externalQueryParam, PNCallback<PNRemoveUuidMetadataResult> callback)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNRemoveUuidMetadataResult> requestState = new RequestState<PNRemoveUuidMetadataResult>
				{
					ResponseType = PNOperationType.PNDeleteUuidMetadataOperation,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDeleteUuidMetadataOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDeleteUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}


		private async Task<PNResult<PNRemoveUuidMetadataResult>> RemoveUuidMetadata(string uuid, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNRemoveUuidMetadataResult> returnValue = new PNResult<PNRemoveUuidMetadataResult>();

			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNRemoveUuidMetadataResult> requestState = new RequestState<PNRemoveUuidMetadataResult>
				{
					ResponseType = PNOperationType.PNDeleteUuidMetadataOperation,
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNRemoveMembershipsOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNRemoveUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNRemoveUuidMetadataResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNRemoveMembershipsOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"uuids",
				string.IsNullOrEmpty(uuid) ? string.Empty : uuid
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNDeleteUuidMetadataOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.DELETE,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}
	}
}
