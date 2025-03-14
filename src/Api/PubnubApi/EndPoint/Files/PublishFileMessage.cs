using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using System.Net;

namespace PubnubApi.EndPoint
{
	public class PublishFileMessageOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;


		private PNCallback<PNPublishFileMessageResult> savedCallback;
		private Dictionary<string, object> queryParam;

		private string channelName;
		private string currentFileId;
		private string currentFileName;
		private object publishMessageContent;
		private bool storeInHistory = true;
		private Dictionary<string, object> userMetadata;
		private string customMessageType;
		private int ttl = -1;

		public PublishFileMessageOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;

		}

		public PublishFileMessageOperation Channel(string channel)
		{
			this.channelName = channel;
			return this;
		}

		public PublishFileMessageOperation Message(object message)
		{
			this.publishMessageContent = message;
			return this;
		}

		public PublishFileMessageOperation ShouldStore(bool store)
		{
			this.storeInHistory = store;
			return this;
		}

		public PublishFileMessageOperation Meta(Dictionary<string, object> metadata)
		{
			this.userMetadata = metadata;
			return this;
		}

		public PublishFileMessageOperation Ttl(int ttl)
		{
			this.ttl = ttl;
			return this;
		}

		public PublishFileMessageOperation FileId(string id)
		{
			this.currentFileId = id;
			return this;
		}

		public PublishFileMessageOperation FileName(string name)
		{
			this.currentFileName = name;
			return this;
		}
		public PublishFileMessageOperation CustomMessageType(string customMessageType)
		{
			this.customMessageType = customMessageType;
			return this;
		}

		public PublishFileMessageOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNPublishFileMessageResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			if (string.IsNullOrEmpty(this.currentFileId) || string.IsNullOrEmpty(this.currentFileName)) {
				throw new ArgumentException("Missing File Id or Name");
			}
			this.savedCallback = callback;
			ProcessFileMessagePublish(this.queryParam, savedCallback);
		}

		public async Task<PNResult<PNPublishFileMessageResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await ProcessFileMessagePublish(this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			ProcessFileMessagePublish(this.queryParam, savedCallback);
		}

		private void ProcessFileMessagePublish(Dictionary<string, object> externalQueryParam, PNCallback<PNPublishFileMessageResult> callback)
		{
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim())) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message"));
				callback.OnResponse(null, status);
				return;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNPublishFileMessageResult> requestState = new RequestState<PNPublishFileMessageResult>
				{
					ResponseType = PNOperationType.PNPublishFileMessageOperation,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNPublishOperation);

			PubnubInstance.transportMiddleware.Send(transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						if (result != null && result.Count >= 3) {
							int publishStatus;
							var _ = int.TryParse(result[0].ToString(), out publishStatus);
							if (publishStatus == 1) {
								ProcessResponseCallbacks(result, requestState);
								logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
							} else {
								PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
								PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishFileMessageResult>(PNOperationType.PNPublishFileMessageOperation, category, requestState, 400, new PNException(responseString));
								requestState.PubnubCallback.OnResponse(default, status);
								logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
							}
						} else {
							ProcessResponseCallbacks(result, requestState);
							logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
						}
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishFileMessageOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		private async Task<PNResult<PNPublishFileMessageResult>> ProcessFileMessagePublish(Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNPublishFileMessageResult> returnValue = new PNResult<PNPublishFileMessageResult>();
			if (string.IsNullOrEmpty(this.currentFileId) || string.IsNullOrEmpty(this.currentFileName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File Id or Name", new ArgumentException("Missing File Id or Name")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim())) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			var requestParameter = CreateRequestParameter();
			RequestState<PNPublishFileMessageResult> requestState = new RequestState<PNPublishFileMessageResult>
				{
					ResponseType = PNOperationType.PNPublishFileMessageOperation,
					Reconnect = false,
					EndPointOperation = this
				};
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNPublishFileMessageOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
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
				List<object> result = ProcessJsonResponse(requestState, json);

				if (result != null && result.Count >= 3) {
					int publishStatus;
					var _ = int.TryParse(result[0].ToString(), out publishStatus);
					if (publishStatus == 1) {
						List<object> resultList = ProcessJsonResponse(requestState, json);
						if (resultList != null && resultList.Count > 0) {
							ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
							PNPublishFileMessageResult responseResult = responseBuilder.JsonToObject<PNPublishFileMessageResult>(resultList, true);
							if (responseResult != null) {
								returnValue.Result = responseResult;
							}
						}
					}
				}
			}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishFileMessageOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			Dictionary<string, object> publishPayload = new Dictionary<string, object>();
			if (this.publishMessageContent != null && !string.IsNullOrEmpty(this.publishMessageContent.ToString())) {
				publishPayload.Add("message", this.publishMessageContent);
			}
			publishPayload.Add("file", new Dictionary<string, string> {
						{ "id", currentFileId },
						{ "name", currentFileName } });
			List<string> pathSegments = new List<string>
			{
				"v1",
				"files",
				"publish-file",
				config.PublishKey,
				config.SubscribeKey,
				"0",
				channelName,
				"0",
				PrepareContent(publishPayload)
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (userMetadata != null) {
				string jsonMetaData = jsonLibrary.SerializeToJsonString(userMetadata);
				requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(jsonMetaData, PNOperationType.PNPublishFileMessageOperation, false, false, false));
			}

			if (storeInHistory && ttl >= 0) {
				requestQueryStringParams.Add("tt1", ttl.ToString(CultureInfo.InvariantCulture));
			}

			if (!storeInHistory) {
				requestQueryStringParams.Add("store", "0");
			}
			
			if (!string.IsNullOrEmpty(customMessageType)) {
				requestQueryStringParams.Add("custom_message_type", customMessageType);
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNPublishFileMessageOperation, false, false, false));
					}
				}
			}
			var requestParameter = new RequestParameter() {
				PathSegment = pathSegments,
				RequestType = Constants.GET,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}

		private string PrepareContent(object originalMessage)
		{
			string message = jsonLibrary.SerializeToJsonString(originalMessage);
			if (config.CryptoModule != null || config.CipherKey.Length > 0) {
				config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, config.UseRandomInitializationVector, config.Logger), null);
				string encryptMessage = config.CryptoModule.Encrypt(message);
				message = jsonLibrary.SerializeToJsonString(encryptMessage);
			}
			return message;
		}

	}
}
