using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace PubnubApi.EndPoint
{
	public class WhereNowOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private string whereNowUUID = string.Empty;
		private PNCallback<PNWhereNowResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public WhereNowOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public WhereNowOperation Uuid(string uuid)
		{
			this.whereNowUUID = uuid;
			return this;
		}

		public WhereNowOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNWhereNowResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNWhereNowResult> callback)
		{
			this.savedCallback = callback;
			WhereNow(this.whereNowUUID, this.queryParam, callback);
		}

		public async Task<PNResult<PNWhereNowResult>> ExecuteAsync()
		{
			return await WhereNow(this.whereNowUUID, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			WhereNow(this.whereNowUUID, this.queryParam, savedCallback);
		}

		internal void WhereNow(string uuid, Dictionary<string, object> externalQueryParam, PNCallback<PNWhereNowResult> callback)
		{
			if (jsonLibrary == null) {
				throw new MissingMemberException("Missing Json Pluggable Library for Pubnub Instance");
			}
			RequestState<PNWhereNowResult> requestState = new RequestState<PNWhereNowResult>();
			requestState.Channels = new[] { whereNowUUID ?? config.UserId };
			requestState.ResponseType = PNOperationType.PNWhereNowOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNWhereNowOperation);
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
						callback.OnResponse(default, errorStatus);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNWhereNowOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		internal async Task<PNResult<PNWhereNowResult>> WhereNow(string uuid, Dictionary<string, object> externalQueryParam)
		{
			if (jsonLibrary == null) {
				throw new MissingMemberException("Missing Json Pluggable Library for Pubnub Instance");
			}

			PNResult<PNWhereNowResult> returnValue = new PNResult<PNWhereNowResult>();
			RequestState<PNWhereNowResult> requestState = new RequestState<PNWhereNowResult>();
			requestState.Channels = new[] { whereNowUUID ?? config.UserId };
			requestState.ResponseType = PNOperationType.PNWhereNowOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNWhereNowOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				requestState.GotJsonResponse = true;
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
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
					PNWhereNowResult responseResult = responseBuilder.JsonToObject<PNWhereNowResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}

			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNWhereNowOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
				"presence",
				"sub_key",
				config.SubscribeKey,
				"uuid",
				string.IsNullOrEmpty(whereNowUUID) ? config.UserId : whereNowUUID
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNWhereNowOperation, false, false, false));
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
