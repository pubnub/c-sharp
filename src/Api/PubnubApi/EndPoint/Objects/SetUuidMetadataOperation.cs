using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class SetUuidMetadataOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string uuidId;
		private string uuidName;
		private string uuidEmail;
		private string uuidExternalId;
		private string uuidProfileUrl;
		private Dictionary<string, object> uuidCustom;
		private bool includeCustom;
		private string ifMatchesEtag = null;

		private PNCallback<PNSetUuidMetadataResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public SetUuidMetadataOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
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

		public SetUuidMetadataOperation Uuid(string uuid)
		{
			this.uuidId = uuid;
			return this;
		}

		public SetUuidMetadataOperation Name(string name)
		{
			this.uuidName = name;
			return this;
		}

		public SetUuidMetadataOperation Email(string email)
		{
			this.uuidEmail = email;
			return this;
		}


		public SetUuidMetadataOperation ExternalId(string externalId)
		{
			this.uuidExternalId = externalId;
			return this;
		}


		public SetUuidMetadataOperation ProfileUrl(string profileUrl)
		{
			this.uuidProfileUrl = profileUrl;
			return this;
		}

		public SetUuidMetadataOperation Custom(Dictionary<string, object> customObject)
		{
			this.uuidCustom = customObject;
			return this;
		}

		public SetUuidMetadataOperation IncludeCustom(bool includeCustomData)
		{
			this.includeCustom = includeCustomData;
			return this;
		}

		public SetUuidMetadataOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}
		public SetUuidMetadataOperation IfMatchesEtag(string etag)
		{
			this.ifMatchesEtag = etag;
			return this;
		}

		public void Execute(PNCallback<PNSetUuidMetadataResult> callback)
		{
			logger.Trace($"{GetType().Name} Execute invoked");
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			this.savedCallback = callback;
			SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNSetUuidMetadataResult>> ExecuteAsync()
		{
			logger.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			SetUuidMetadata(this.uuidId, this.includeCustom, this.queryParam, savedCallback);
		}

		private void SetUuidMetadata(string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNSetUuidMetadataResult> callback)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuidId = config.UserId;
			}
			logger.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNSetUuidMetadataResult> requestState = new RequestState<PNSetUuidMetadataResult>
				{
					ResponseType = PNOperationType.PNSetUuidMetadataOperation,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this,
					UsePatchMethod = true
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSetUuidMetadataOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
                        logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						callback.OnResponse(default, errorStatus);
					}

				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSetUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNSetUuidMetadataResult>> SetUuidMetadata(string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam)
		{
			if (string.IsNullOrEmpty(uuid)) {
				this.uuidId = config.UserId;
			}
			logger.Debug($"{GetType().Name} parameter validated.");
			PNResult<PNSetUuidMetadataResult> returnValue = new PNResult<PNSetUuidMetadataResult>();

			RequestState<PNSetUuidMetadataResult> requestState = new RequestState<PNSetUuidMetadataResult>
				{
					ResponseType = PNOperationType.PNSetUuidMetadataOperation,
					Reconnect = false,
					EndPointOperation = this,
					UsePatchMethod = true
				};

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNSetUuidMetadataOperation);
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
					PNSetUuidMetadataResult responseResult = responseBuilder.JsonToObject<PNSetUuidMetadataResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNSetUuidMetadataOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
			if (uuidName != null) {
				messageEnvelope.Add("name", uuidName);
			}
			if (uuidExternalId != null) {
				messageEnvelope.Add("externalId", uuidExternalId);
			}
			if (uuidProfileUrl != null) {
				messageEnvelope.Add("profileUrl", uuidProfileUrl);
			}
			if (uuidEmail != null) {
				messageEnvelope.Add("email", uuidEmail);
			}
			if (uuidCustom != null) {
				messageEnvelope.Add("custom", uuidCustom);
			}
			string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

			List<string> pathSegments = new List<string>
			{
				"v2",
				"objects",
				config.SubscribeKey,
				"uuids",
				string.IsNullOrEmpty(uuidId) ? string.Empty : uuidId
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (includeCustom) {
				requestQueryStringParams.Add("include", "custom");
			}
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNSetUuidMetadataOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
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
