using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class DeleteFileOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private PNCallback<PNDeleteFileResult> savedCallback;
		private Dictionary<string, object> queryParam;

		private string channelName;
		private string fileId;
		private string fileName;

		public DeleteFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public DeleteFileOperation Channel(string channel)
		{
			this.channelName = channel;
			return this;
		}

		public DeleteFileOperation FileId(string fileId)
		{
			this.fileId = fileId;
			return this;
		}

		public DeleteFileOperation FileName(string fileName)
		{
			this.fileName = fileName;
			return this;
		}

		public DeleteFileOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNDeleteFileResult> callback)
		{
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}
			if (string.IsNullOrEmpty(this.channelName)) {
				throw new ArgumentException("Missing Channel Name");
			}
			if (string.IsNullOrEmpty(this.fileId)) {
				throw new ArgumentException("Missing File Id");
			}
			if (string.IsNullOrEmpty(this.fileName)) {
				throw new ArgumentException("Missing File Name");
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			this.savedCallback = callback;
			logger?.Trace($"{GetType().Name} Execute invoked");
			ProcessDeleteFileRequest(this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNDeleteFileResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await ProcessDeleteFileRequest(this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			ProcessDeleteFileRequest(this.queryParam, savedCallback);
		}

		private void ProcessDeleteFileRequest(Dictionary<string, object> externalQueryParam, PNCallback<PNDeleteFileResult> callback)
		{
			RequestState<PNDeleteFileResult> requestState = new RequestState<PNDeleteFileResult>
			{
				ResponseType = PNOperationType.PNDeleteFileOperation,
				PubnubCallback = callback,
				UsePostMethod = false,
				Reconnect = false,
				EndPointOperation = this
			};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDeleteFileOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					requestState.GotJsonResponse = true;
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
						callback.OnResponse(default, errorStatus);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDeleteFileOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNDeleteFileResult>> ProcessDeleteFileRequest(Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNDeleteFileResult> returnValue = new PNResult<PNDeleteFileResult>();

			if (string.IsNullOrEmpty(this.channelName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Channel name", new ArgumentException("Invalid Channel name")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			if (string.IsNullOrEmpty(this.fileId)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File Id", new ArgumentException("Missing File Id")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(this.fileName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			var requestParameter = CreateRequestParameter();
			RequestState<PNDeleteFileResult> requestState = new RequestState<PNDeleteFileResult>();
			requestState.ResponseType = PNOperationType.PNDeleteFileOperation;
			requestState.Reconnect = false;
			requestState.UsePostMethod = false;
			requestState.EndPointOperation = this;
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDeleteFileOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				requestState.GotJsonResponse = true;
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
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNDeleteFileResult responseResult = responseBuilder.JsonToObject<PNDeleteFileResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDeleteFileOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

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
				"files",
				fileId,
				fileName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNDeleteFileOperation, false, false, false));
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
