﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class GetUuidMetadataOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private bool includeCustom;
		private string uuid = string.Empty;

		private PNCallback<PNGetUuidMetadataResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public GetUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public GetUuidMetadataOperation Uuid(string uuid)
		{
			this.uuid = uuid;
			return this;
		}

		public GetUuidMetadataOperation IncludeCustom(bool includeCustomData)
		{
			this.includeCustom = includeCustomData;
			return this;
		}

		public GetUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNGetUuidMetadataResult> callback)
		{
			this.savedCallback = callback;
			GetSingleUuidMetadata(this.uuid, this.includeCustom, this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNGetUuidMetadataResult>> ExecuteAsync()
		{
			return await GetSingleUuidMetadata(this.uuid, this.includeCustom, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			GetSingleUuidMetadata(this.uuid, this.includeCustom, this.queryParam, savedCallback);
		}

		private void GetSingleUuidMetadata(string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNGetUuidMetadataResult> callback)
		{
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			RequestState<PNGetUuidMetadataResult> requestState = new RequestState<PNGetUuidMetadataResult>();
			requestState.ResponseType = PNOperationType.PNGetUuidMetadataOperation;
			requestState.PubnubCallback = callback;
			requestState.UsePostMethod = false;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetUuidMetadataOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(null, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNGetUuidMetadataResult), status);
				}
			});
		}

		private async Task<PNResult<PNGetUuidMetadataResult>> GetSingleUuidMetadata(string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuid = config.UserId;
			}
			PNResult<PNGetUuidMetadataResult> returnValue = new PNResult<PNGetUuidMetadataResult>();
			RequestState<PNGetUuidMetadataResult> requestState = new RequestState<PNGetUuidMetadataResult>();
			requestState.ResponseType = PNOperationType.PNGetUuidMetadataOperation;
			requestState.Reconnect = false;
			requestState.UsePostMethod = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetUuidMetadataOperation);
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
					PNGetUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNGetUuidMetadataResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

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
			if (includeCustom) {
				requestQueryStringParams.Add("include", "custom");
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetUuidMetadataOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}
	}
}
