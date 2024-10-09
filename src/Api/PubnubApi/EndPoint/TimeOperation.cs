using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class TimeOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private Dictionary<string, object> queryParam;

		private PNCallback<PNTimeResult> savedCallback;

		public TimeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, null, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public TimeOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNTimeResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNTimeResult> callback)
		{
            this.savedCallback = callback;
            Time(this.queryParam, callback);
		}

		public async Task<PNResult<PNTimeResult>> ExecuteAsync()
		{
			return await Time(this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			Time(this.queryParam, savedCallback);
		}

		internal void Time(Dictionary<string, object> externalQueryParam, PNCallback<PNTimeResult> callback)
		{
			RequestState<PNTimeResult> requestState = new RequestState<PNTimeResult>();
			requestState.Channels = null;
			requestState.ResponseType = PNOperationType.PNTimeOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNTimeOperation);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNTimeOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		internal async Task<PNResult<PNTimeResult>> Time(Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNTimeResult> returnValue = new PNResult<PNTimeResult>();
			RequestState<PNTimeResult> requestState = new RequestState<PNTimeResult>();
			requestState.ResponseType = PNOperationType.PNTimeOperation;
			requestState.Channels = null;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNTimeOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse<PNTimeResult>(requestState, json);
					if (resultList != null && resultList.Count > 0) {
						ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
						PNTimeResult responseResult = responseBuilder.JsonToObject<PNTimeResult>(resultList, true);
						if (responseResult != null) {
							returnValue.Result = responseResult;
						}
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNTimeOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
				"time",
				"0"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNTimeOperation, false, false, false));
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
