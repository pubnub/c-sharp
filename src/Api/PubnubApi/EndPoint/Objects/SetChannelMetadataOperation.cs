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
		private readonly IPubnubLog pubnubLog;

		private string channelId = string.Empty;
		private string channelName;
		private string channelDescription;
		private Dictionary<string, object> custom;
		private bool includeCustom;

		private PNCallback<PNSetChannelMetadataResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public SetChannelMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public SetChannelMetadataOperation Channel(string channelName)
		{
			this.channelId = channelName;
			return this;
		}

		public SetChannelMetadataOperation Name(string channelMetadataName)
		{
			this.channelName = channelMetadataName;
			return this;
		}

		public SetChannelMetadataOperation Description(string channelMetadataDescription)
		{
			this.channelDescription = channelMetadataDescription;
			return this;
		}

		public SetChannelMetadataOperation Custom(Dictionary<string, object> channelMetadataCustomObject)
		{
			this.custom = channelMetadataCustomObject;
			return this;
		}

		public SetChannelMetadataOperation IncludeCustom(bool includeCustomData)
		{
			this.includeCustom = includeCustomData;
			return this;
		}


		public SetChannelMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNSetChannelMetadataResult> callback)
		{
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(channelId.Trim())) {
				throw new ArgumentException("Missing Channel");
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				throw new MissingMemberException("Invalid subscribe key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}

			this.savedCallback = callback;
			SetChannelMetadata(this.channelId, this.includeCustom, this.queryParam, callback);
		}

		public async Task<PNResult<PNSetChannelMetadataResult>> ExecuteAsync()
		{
			return await SetChannelMetadata(this.channelId, this.includeCustom, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			SetChannelMetadata(this.channelId, this.includeCustom, this.queryParam, savedCallback);
		}

		private void SetChannelMetadata(string channelMetaId, bool includeCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNSetChannelMetadataResult> callback)
		{
			RequestState<PNSetChannelMetadataResult> requestState = new RequestState<PNSetChannelMetadataResult>();
			requestState.ResponseType = PNOperationType.PNSetChannelMetadataOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePatchMethod = true;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSetChannelMetadataOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSetChannelMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNSetChannelMetadataResult>> SetChannelMetadata(string channelMetaId, bool includeCustom, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNSetChannelMetadataResult> returnValue = new PNResult<PNSetChannelMetadataResult>();

			if (string.IsNullOrEmpty(channelMetaId) || string.IsNullOrEmpty(channelMetaId.Trim())) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}


			RequestState<PNSetChannelMetadataResult> requestState = new RequestState<PNSetChannelMetadataResult>();
			requestState.ResponseType = PNOperationType.PNSetChannelMetadataOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePatchMethod = true;

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSetChannelMetadataOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 200, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNSetChannelMetadataResult responseResult = responseBuilder.JsonToObject<PNSetChannelMetadataResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSetChannelMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}


			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
			if (this.channelName != null) {
				messageEnvelope.Add("name", channelName);
			}
			if (this.channelDescription != null) {
				messageEnvelope.Add("description", channelDescription);
			}
			if (this.custom != null) {
				messageEnvelope.Add("custom", custom);
			}
			string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

			List<string> pathSegments = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"channels",
				string.IsNullOrEmpty(channelId) ? string.Empty : channelId
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (includeCustom) {
				requestQueryStringParams.Add("include", "custom");
			}
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNSetChannelMetadataOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.PATCH,
				PathSegment = pathSegments,
				Query = requestQueryStringParams,
				BodyContentString = patchMessage
			};

			return requestParameter;
		}
	}
}
