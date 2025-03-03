using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class RevokeTokenOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string pnToken = string.Empty;
		private PNCallback<PNAccessManagerRevokeTokenResult> savedCallbackRevokeToken;
		private Dictionary<string, object> queryParam;

		public RevokeTokenOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
			PubnubInstance = instance;

			InitializeDefaultVariableObjectStates();
		}

		public RevokeTokenOperation Token(string tokenToBeRevoked)
		{
			this.pnToken = tokenToBeRevoked;
			return this;
		}

		public RevokeTokenOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNAccessManagerRevokeTokenResult> callback)
		{
			this.savedCallbackRevokeToken = callback;
			logger?.Trace($"{GetType().Name} Execute invoked");
			RevokeAccess(callback);
		}

		public async Task<PNResult<PNAccessManagerRevokeTokenResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await RevokeAccess().ConfigureAwait(false);
		}

		internal void Retry()
		{
			RevokeAccess(savedCallbackRevokeToken);
		}

		internal void RevokeAccess(PNCallback<PNAccessManagerRevokeTokenResult> callback)
		{
			if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0) {
				throw new MissingMemberException("Invalid secret key");
			}

			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNAccessManagerRevokeTokenResult> requestState = new RequestState<PNAccessManagerRevokeTokenResult>
				{
					ResponseType = PNOperationType.PNAccessManagerRevokeToken,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerRevokeToken);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					var responseString = Encoding.UTF8.GetString(t.Result.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(t.Result.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, t.Result.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerRevokeToken, category, requestState, statusCode, new PNException(t.Result.Error.Message, t.Result.Error));
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default(PNAccessManagerRevokeTokenResult), status);
				}
			});
		}

		internal async Task<PNResult<PNAccessManagerRevokeTokenResult>> RevokeAccess()
		{
			if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0) {
				throw new MissingMemberException("Invalid secret key");
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			PNResult<PNAccessManagerRevokeTokenResult> returnValue = new PNResult<PNAccessManagerRevokeTokenResult>();
			RequestState<PNAccessManagerRevokeTokenResult> requestState = new RequestState<PNAccessManagerRevokeTokenResult>();
			requestState.ResponseType = PNOperationType.PNAccessManagerRevokeToken;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerRevokeToken);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				requestState.GotJsonResponse = true;
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null  && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					if (resultList != null && resultList.Count > 0) {
						ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
						PNAccessManagerRevokeTokenResult responseResult = responseBuilder.JsonToObject<PNAccessManagerRevokeTokenResult>(resultList, true);
						if (responseResult != null) {
							returnValue.Result = responseResult;
						}
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerRevokeToken, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));logger?.Debug($"{GetType().Name} request finished with status code {statusCode}");
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>() {
				"v3",
				"pam",
				config.SubscribeKey,
				"grant",
				pnToken
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.ChannelGroupRevokeAccess, false, false, false));
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
