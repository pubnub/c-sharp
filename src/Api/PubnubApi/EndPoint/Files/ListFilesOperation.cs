using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PubnubApi.EndPoint
{
	public class ListFilesOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private PNCallback<PNListFilesResult> savedCallback;
		private Dictionary<string, object> queryParam;

		private string channelName;
		private string nextFileBatchToken;
		private int limitFileCount = -1;

		public ListFilesOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public ListFilesOperation Channel(string channel)
		{
			this.channelName = channel;
			return this;
		}

		public ListFilesOperation Limit(int count)
		{
			this.limitFileCount = count;
			return this;
		}

		public ListFilesOperation Next(string nextToken)
		{
			this.nextFileBatchToken = nextToken;
			return this;
		}

		public ListFilesOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNListFilesResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}
			if (string.IsNullOrEmpty(this.channelName)) {
				throw new ArgumentException("Missing Channel Name");
			}
			this.savedCallback = callback;
			logger?.Debug($"{GetType().Name} parameter validated.");
			ProcessListFilesRequest(savedCallback);
		}

		public async Task<PNResult<PNListFilesResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await ProcessListFilesRequest().ConfigureAwait(false);
		}

		internal void Retry()
		{
			ProcessListFilesRequest(savedCallback);
		}

		private void ProcessListFilesRequest(PNCallback<PNListFilesResult> callback)
		{
			RequestState<PNListFilesResult> requestState = new RequestState<PNListFilesResult>
			{
				ResponseType = PNOperationType.PNListFilesOperation,
				PubnubCallback = callback,
				UsePostMethod = false,
				Reconnect = false,
				EndPointOperation = this
			};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNListFilesOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNListFilesOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		private async Task<PNResult<PNListFilesResult>> ProcessListFilesRequest()
		{
			PNResult<PNListFilesResult> returnValue = new PNResult<PNListFilesResult>();

			if (string.IsNullOrEmpty(this.channelName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Channel name", new ArgumentException("Invalid Channel name")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNListFilesResult> requestState = new RequestState<PNListFilesResult>
			{
				ResponseType = PNOperationType.PNListFilesOperation,
				Reconnect = false,
				EndPointOperation = this,
				UsePostMethod = false
			};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNListFilesOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				Tuple<string, PNStatus> jsonAndStatusTuple;
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					jsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = jsonAndStatusTuple.Item2;
				string json = jsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNListFilesResult responseResult = responseBuilder.JsonToObject<PNListFilesResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNListFilesOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			List<string> pathSegments = new List<string>
			{
				"v1",
				"files",
				config.SubscribeKey,
				"channels",
				channelName,
				"files"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>
			{
				{ "limit", (limitFileCount <= -1) ? "100" : limitFileCount.ToString(CultureInfo.InvariantCulture) }
			};
			if (!string.IsNullOrEmpty(nextFileBatchToken)) {
				requestQueryStringParams.Add("next", nextFileBatchToken);
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNListFilesOperation, false, false, false));
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
