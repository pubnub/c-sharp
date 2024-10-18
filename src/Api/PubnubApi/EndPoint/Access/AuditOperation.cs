using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class AuditOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string channelName;
		private string channelGroupName;
		private string[] authenticationKeys;
		private PNCallback<PNAccessManagerAuditResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public AuditOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, null, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public AuditOperation Channel(string channel)
		{
			this.channelName = channel;
			return this;
		}

		public AuditOperation ChannelGroup(string channelGroup)
		{
			this.channelGroupName = channelGroup;
			return this;
		}

		public AuditOperation AuthKeys(string[] authKeys)
		{
			this.authenticationKeys = authKeys;
			return this;
		}

		public AuditOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNAccessManagerAuditResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNAccessManagerAuditResult> callback)
		{
            this.savedCallback = callback;
            AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam, callback);
		}


		public async Task<PNResult<PNAccessManagerAuditResult>> ExecuteAsync()
		{
			return await AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, this.queryParam, savedCallback);
		}

		internal void AuditAccess(string channel, string channelGroup, string[] authKeys, Dictionary<string, object> externalQueryParam, PNCallback<PNAccessManagerAuditResult> callback)
		{
			if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0) {
				throw new MissingMemberException("Invalid secret key");
			}
			RequestState<PNAccessManagerAuditResult> requestState = new RequestState<PNAccessManagerAuditResult>();
			if (!string.IsNullOrEmpty(channel)) {
				requestState.Channels = new[] { channel };
			}
			if (!string.IsNullOrEmpty(channelGroup)) {
				requestState.ChannelGroups = new[] { channelGroup };
			}
			requestState.ResponseType = PNOperationType.PNAccessManagerAudit;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerAudit);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					requestState.GotJsonResponse = true;
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerAudit, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});

		}

		internal async Task<PNResult<PNAccessManagerAuditResult>> AuditAccess(string channel, string channelGroup, string[] authKeys, Dictionary<string, object> externalQueryParam)
		{
			if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0) {
				throw new MissingMemberException("Invalid secret key");
			}

			PNResult<PNAccessManagerAuditResult> returnValue = new PNResult<PNAccessManagerAuditResult>();
			RequestState<PNAccessManagerAuditResult> requestState = new RequestState<PNAccessManagerAuditResult>();
			requestState.ResponseType = PNOperationType.PNAccessManagerAudit;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			if (!string.IsNullOrEmpty(channel)) {
				requestState.Channels = new[] { channel };
			}
			if (!string.IsNullOrEmpty(channelGroup)) {
				requestState.ChannelGroups = new[] { channelGroup };
			}
			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerAudit);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null)
				{
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> result = ProcessJsonResponse(requestState, json);
					if (result != null) {
						List<object> resultList = ProcessJsonResponse(requestState, json);
						if (resultList != null && resultList.Count > 0) {
							ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
							PNAccessManagerAuditResult responseResult = responseBuilder.JsonToObject<PNAccessManagerAuditResult>(resultList, true);
							if (responseResult != null) {
								returnValue.Result = responseResult;
							}
						}
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerAudit, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
			PubnubInstance = instance;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>
			{
				"v2",
				"auth",
				"audit",
				"sub-key",
				config.SubscribeKey
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			string authKeysCommaDelimited = (authenticationKeys != null && authenticationKeys.Length > 0) ? string.Join(",", authenticationKeys.OrderBy(x => x).ToArray()) : string.Empty;
			if (!string.IsNullOrEmpty(authKeysCommaDelimited)) {
				requestQueryStringParams.Add("auth", UriUtil.EncodeUriComponent(authKeysCommaDelimited, PNOperationType.PNAccessManagerAudit, false, false, false));
			}

			if (!string.IsNullOrEmpty(channelName)) {
				requestQueryStringParams.Add("channel", UriUtil.EncodeUriComponent(channelName, PNOperationType.PNAccessManagerAudit, false, false, false));
			}

			if (!string.IsNullOrEmpty(channelGroupName)) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroupName, PNOperationType.PNAccessManagerAudit, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNAccessManagerAudit, false, false, false));
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
