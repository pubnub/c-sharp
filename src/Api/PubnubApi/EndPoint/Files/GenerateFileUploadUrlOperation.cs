using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	internal class GenerateFileUploadUrlOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private Dictionary<string, object> queryParam;

		private string channelName;
		private string sendFileName;

		public GenerateFileUploadUrlOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
			PubnubInstance = instance;
		}

		public GenerateFileUploadUrlOperation Channel(string channel)
		{
			this.channelName = channel;
			return this;
		}

		public GenerateFileUploadUrlOperation FileName(string fileName)
		{
			this.sendFileName = fileName;
			return this;
		}

		public GenerateFileUploadUrlOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNGenerateFileUploadUrlResult> callback)
		{
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			if (string.IsNullOrEmpty(this.sendFileName)) {
				throw new ArgumentException("Missing File Name");
			}

			GenerateFileUploadUrl(this.queryParam, callback);
		}

		public async Task<PNResult<PNGenerateFileUploadUrlResult>> ExecuteAsync()
		{
			return await GenerateFileUploadUrl(this.queryParam).ConfigureAwait(false);
		}

		private void GenerateFileUploadUrl(Dictionary<string, object> externalQueryParam, PNCallback<PNGenerateFileUploadUrlResult> callback)
		{
			RequestState<PNGenerateFileUploadUrlResult> requestState = new RequestState<PNGenerateFileUploadUrlResult>();
			requestState.ResponseType = PNOperationType.PNGenerateFileUploadUrlOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.UsePostMethod = true;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGenerateFileUploadUrlOperation);
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
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGenerateFileUploadUrlOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		private async Task<PNResult<PNGenerateFileUploadUrlResult>> GenerateFileUploadUrl(Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNGenerateFileUploadUrlResult> returnValue = new PNResult<PNGenerateFileUploadUrlResult>();
			if (string.IsNullOrEmpty(sendFileName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name")) };
				returnValue.Status = errStatus;
				return returnValue;
			}


			RequestState<PNGenerateFileUploadUrlResult> requestState = new RequestState<PNGenerateFileUploadUrlResult>();
			requestState.ResponseType = PNOperationType.PNGenerateFileUploadUrlOperation;
			requestState.Reconnect = false;
			requestState.UsePostMethod = true;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGenerateFileUploadUrlOperation);
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
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNGenerateFileUploadUrlResult responseResult = responseBuilder.JsonToObject<PNGenerateFileUploadUrlResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGenerateFileUploadUrlOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
				"generate-upload-url"
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGenerateFileUploadUrlOperation, false, false, false));
					}
				}
			}

			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
			if (!string.IsNullOrEmpty(sendFileName)) {
				messageEnvelope.Add("name", sendFileName);
			}
			string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

			var requestParameter = new RequestParameter() {
				RequestType = Constants.POST,
				PathSegment = pathSegments,
				Query = requestQueryStringParams,
				BodyContentString = postMessage
			};
			return requestParameter;
		}
	}
}
