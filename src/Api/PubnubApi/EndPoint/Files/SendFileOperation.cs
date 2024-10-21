using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi.EndPoint
{
	public class SendFileOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private Dictionary<string, object> queryParam;

		private string channelName;
		private object publishFileMessageContent;
		private string sendFileFullPath;
		private string sendFileName = string.Empty;
		private byte[] sendFileBytes = null;
		private string currentFileCipherKey;
		private string currentFileId;
		private bool storeInHistory = true;
		private Dictionary<string, object> userMetadata;
		private int ttl = -1;

		public SendFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public SendFileOperation Channel(string channel)
		{
			channelName = channel;
			return this;
		}

		public SendFileOperation Message(object message)
		{
			publishFileMessageContent = message;
			return this;
		}

		public SendFileOperation ShouldStore(bool store)
		{
			storeInHistory = store;
			return this;
		}

		public SendFileOperation Meta(Dictionary<string, object> metadata)
		{
			userMetadata = metadata;
			return this;
		}

		public SendFileOperation Ttl(int ttl)
		{
			this.ttl = ttl;
			return this;
		}

		public SendFileOperation File(string fileNameWithFullPath)
		{
			sendFileFullPath = fileNameWithFullPath;

			if (System.IO.File.Exists(fileNameWithFullPath) && string.IsNullOrEmpty(sendFileName)) {
				sendFileName = Path.GetFileName(fileNameWithFullPath);
			}
			return this;
		}

		public SendFileOperation File(byte[] byteArray)
		{
			sendFileBytes = byteArray ?? throw new ArgumentException("File byte array not provided.");
			return this;
		}

		public SendFileOperation FileName(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) {
				throw new ArgumentException("File name is missing");
			}

			if (fileName.Trim() != fileName) {
				throw new ArgumentException("File name should not contain leading or trailing whitespace");
			}

			sendFileName = fileName;
			return this;
		}

		public SendFileOperation CipherKey(string cipherKeyForFile)
		{
			currentFileCipherKey = cipherKeyForFile;
			return this;
		}

		public SendFileOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNFileUploadResult> callback)
		{
			if (callback == null) {
				throw new ArgumentException("Missing callback");
			}

			if (string.IsNullOrEmpty(sendFileName)) {
				throw new ArgumentException("Missing File");
			}
			ProcessFileUpload(callback);
		}

		public async Task<PNResult<PNFileUploadResult>> ExecuteAsync()
		{
			return await ProcessFileUpload();
		}

		private void ProcessFileUpload(PNCallback<PNFileUploadResult> callback)
		{
			PNResult<PNGenerateFileUploadUrlResult> generateFileUploadUrl = GenerateFileUploadUrl().Result;
			PNGenerateFileUploadUrlResult generateFileUploadUrlResult = generateFileUploadUrl.Result;
			PNStatus generateFileUploadUrlStatus = generateFileUploadUrl.Status;

			if (generateFileUploadUrlStatus.Error || generateFileUploadUrlResult == null) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Error in GenerateFileUploadUrl. Try again.", new ArgumentException("Error in GenerateFileUploadUrl. Try again.")) };
				if (callback != null) {
					callback.OnResponse(null, errStatus);
				}
				return;
			}
			LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl executed.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
			RequestState<PNFileUploadResult> requestState = new RequestState<PNFileUploadResult>();
			requestState.ResponseType = PNOperationType.PNFileUploadOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePostMethod = true;
			byte[] sendFileByteArray = sendFileBytes ?? GetByteArrayFromFilePath(sendFileFullPath);
			string dataBoundary = string.Format(CultureInfo.InvariantCulture, "----------{0:N}", Guid.NewGuid());
			string contentType = "multipart/form-data; boundary=" + dataBoundary;
			CryptoModule currentCryptoModule = null;
			if (!string.IsNullOrEmpty(currentFileCipherKey) || !string.IsNullOrEmpty(config.CipherKey) || config.CryptoModule != null) {
				currentCryptoModule = !string.IsNullOrEmpty(currentFileCipherKey) ? new CryptoModule(new LegacyCryptor(currentFileCipherKey, true, pubnubLog), null) : (config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, true, pubnubLog), null));
			}
			byte[] postData = GetMultipartFormData(sendFileByteArray, generateFileUploadUrlResult.FileName, generateFileUploadUrlResult.FileUploadRequest.FormFields, dataBoundary, currentCryptoModule, config, pubnubLog);
			var transportRequest = new TransportRequest() {
				RequestType = Constants.POST,
				RequestUrl = generateFileUploadUrlResult.FileUploadRequest.Url,
				BodyContentBytes = postData,
			};
			transportRequest.Headers.Add("Content-Type", contentType);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					if (transportResponse.StatusCode == 204 && transportResponse.Error == null)
					{
						requestState.GotJsonResponse = true;
						LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} file upload request executed.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
						Dictionary<string, object> publishPayload = new Dictionary<string, object>();
						if (publishFileMessageContent != null && !string.IsNullOrEmpty(publishFileMessageContent.ToString())) {
							publishPayload.Add("message", publishFileMessageContent);
						}
						currentFileId = generateFileUploadUrlResult.FileId;
						sendFileName = generateFileUploadUrlResult.FileName;
						publishPayload.Add("file", new Dictionary<string, string> {
						{ "id", generateFileUploadUrlResult.FileId },
						{ "name", generateFileUploadUrlResult.FileName } });
						int publishFileRetryLimit = config.FileMessagePublishRetryLimit;
						int currentFileRetryCount = 0;
						bool publishFailed;
						do {
							currentFileRetryCount += 1;
							PNResult<PNPublishFileMessageResult> publishFileMessageResponse = PublishFileMessage(publishPayload, queryParam).Result;
							PNPublishFileMessageResult publishFileMessage = publishFileMessageResponse.Result;
							PNStatus publishFileMessageStatus = publishFileMessageResponse.Status;
							if (publishFileMessageStatus != null && !publishFileMessageStatus.Error && publishFileMessage != null) {
								publishFailed = false;
								PNFileUploadResult result = new PNFileUploadResult();
								result.Timetoken = publishFileMessage.Timetoken;
								result.FileId = generateFileUploadUrlResult.FileId;
								result.FileName = generateFileUploadUrlResult.FileName;
								LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload -> PublishFileMessage -> OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
								var status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 200, null);
								callback.OnResponse(result, status);
							} else {
								publishFailed = true;
								if (currentFileRetryCount == publishFileRetryLimit) {
									callback.OnResponse(null, publishFileMessageStatus);
								}
								LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} PublishFileMessage Failed. currentFileRetryCount={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentFileRetryCount), config.LogVerbosity);
							}
						}
						while (publishFailed && currentFileRetryCount <= publishFileRetryLimit);
					} else {
						int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
						PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
						PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNFileUploadOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
						requestState.PubnubCallback.OnResponse(default, status);
					}
				}
			});
		}

		private async Task<PNResult<PNFileUploadResult>> ProcessFileUpload()
		{
			PNResult<PNFileUploadResult> returnValue = new PNResult<PNFileUploadResult>();
			if (string.IsNullOrEmpty(sendFileName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File", new ArgumentException("Missing File")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			PNResult<PNGenerateFileUploadUrlResult> generateFileUploadUrl = await GenerateFileUploadUrl().ConfigureAwait(false);
			PNGenerateFileUploadUrlResult generateFileUploadUrlResult = generateFileUploadUrl.Result;
			PNStatus generateFileUploadUrlStatus = generateFileUploadUrl.Status;
			if (generateFileUploadUrlStatus.Error || generateFileUploadUrlResult == null) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Error in GenerateFileUploadUrl.", new ArgumentException("Error in GenerateFileUploadUrl.")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl executed.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
			RequestState<PNFileUploadResult> requestState = new RequestState<PNFileUploadResult>();
			requestState.ResponseType = PNOperationType.PNFileUploadOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			requestState.UsePostMethod = true;
			byte[] sendFileByteArray = sendFileBytes ?? GetByteArrayFromFilePath(sendFileFullPath);
			string dataBoundary = string.Format(CultureInfo.InvariantCulture, "----------{0:N}", Guid.NewGuid());
			string contentType = "multipart/form-data; boundary=" + dataBoundary;
			CryptoModule currentCryptoModule = null;
			if (!string.IsNullOrEmpty(currentFileCipherKey) || !string.IsNullOrEmpty(config.CipherKey) || config.CryptoModule != null) {
				currentCryptoModule = !string.IsNullOrEmpty(currentFileCipherKey) ? new CryptoModule(new LegacyCryptor(currentFileCipherKey, true, pubnubLog), null) : (config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, true, pubnubLog), null));
			}
			byte[] postData = GetMultipartFormData(sendFileByteArray, generateFileUploadUrlResult.FileName, generateFileUploadUrlResult.FileUploadRequest.FormFields, dataBoundary, currentCryptoModule, config, pubnubLog);
			var transportRequest = new TransportRequest() {
				RequestType = Constants.POST,
				RequestUrl = generateFileUploadUrlResult.FileUploadRequest.Url,
				BodyContentBytes = postData,
			};
			transportRequest.Headers.Add("Content-Type", contentType);
			Tuple<string, PNStatus> jsonAndStatusTuple;
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.StatusCode == 204 && transportResponse.Error == null) {
				var responseString = "{}";
				PNStatus errStatus = GetStatusIfError<PNFileUploadResult>(requestState, responseString);
				if (errStatus == null) {
					requestState.GotJsonResponse =true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 200, null);
					jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					jsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errStatus);
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNFileUploadOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				jsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, status);
			}
			returnValue.Status = jsonAndStatusTuple.Item2;
			string json = jsonAndStatusTuple.Item1;
			if (!string.IsNullOrEmpty(json)) {
				LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);                //do internal publish after successful file upload

				Dictionary<string, object> publishPayload = new Dictionary<string, object>();
				if (publishFileMessageContent != null && !string.IsNullOrEmpty(publishFileMessageContent.ToString())) {
					publishPayload.Add("message", publishFileMessageContent);
				}
				currentFileId = generateFileUploadUrlResult.FileId;
				sendFileName = generateFileUploadUrlResult.FileName;
				publishPayload.Add("file", new Dictionary<string, string> {
						{ "id", generateFileUploadUrlResult.FileId },
						{ "name", generateFileUploadUrlResult.FileName } });

				int publishFileRetryLimit = config.FileMessagePublishRetryLimit;
				int currentFileRetryCount = 0;
				bool publishFailed;
				do {
					currentFileRetryCount += 1;
					PNResult<PNPublishFileMessageResult> publishFileMessageResponse = await PublishFileMessage(publishPayload, queryParam).ConfigureAwait(false);
					PNPublishFileMessageResult publishFileMessage = publishFileMessageResponse.Result;
					PNStatus publishFileMessageStatus = publishFileMessageResponse.Status;
					if (publishFileMessageStatus != null && !publishFileMessageStatus.Error && publishFileMessage != null) {
						publishFailed = false;
						PNFileUploadResult result = new PNFileUploadResult
						{
							Timetoken = publishFileMessage.Timetoken,
							FileId = generateFileUploadUrlResult.FileId,
							FileName = generateFileUploadUrlResult.FileName
						};
						returnValue.Result = result;
						if (returnValue.Status != null) returnValue.Status.Error = false;
						LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload -> PublishFileMessage -> Success.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
					} else {
						publishFailed = true;
						returnValue.Status = publishFileMessageStatus;
						LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} PublishFileMessage Failed. currentFileRetryCount={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentFileRetryCount), config.LogVerbosity);
						await Task.Delay(1000);
					}
				}
				while (publishFailed && currentFileRetryCount <= publishFileRetryLimit);
			}

			return returnValue;
		}

		private async Task<PNResult<PNGenerateFileUploadUrlResult>> GenerateFileUploadUrl()
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

			var requestParameter = CreateFileUploadUrlRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGenerateFileUploadUrlOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				Tuple<string, PNStatus> jsonAndStatusTuple;
				if (errorStatus == null)
				{
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

		private async Task<PNResult<PNPublishFileMessageResult>> PublishFileMessage(object message, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNPublishFileMessageResult> returnValue = new PNResult<PNPublishFileMessageResult>();

			var requestParameter = CreatePublishFileMessageRequestParameter();
			RequestState<PNPublishFileMessageResult> requestState = new RequestState<PNPublishFileMessageResult>();
			requestState.Channels = new[] { channelName };
			requestState.ResponseType = PNOperationType.PNPublishFileMessageOperation;
			requestState.PubnubCallback = null;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNPublishFileMessageOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest);
			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
				if (!string.IsNullOrEmpty(responseString)) {
					requestState.GotJsonResponse = true;
					List<object> result = ProcessJsonResponse(requestState, responseString);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
					PNPublishFileMessageResult publishResult = responseBuilder.JsonToObject<PNPublishFileMessageResult>(result, true);
					StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
					if (publishResult != null) {
						returnValue.Result = publishResult;
						PNStatus status = statusBuilder.CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
						returnValue.Status = status;
					} else {
						PNException ex = new PNException("File has been upload but the notification couldn't be sent to the subscribed users");
						PNStatus status = statusBuilder.CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNUnknownCategory, requestState, 400, ex);
						status.AdditonalData = new Dictionary<string, string> { { "FileId", currentFileId }, { "FileName", sendFileName } };
						returnValue.Status = status;
					}
				} else {
					returnValue.Status = GetStatusIfError(requestState, responseString);
					if (returnValue.Status == null) {
						PNException ex = new PNException("PublishFileMessage failed.");
						StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
						PNStatus status = statusBuilder.CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNUnknownCategory, requestState, 400, ex);
						status.AdditonalData = new Dictionary<string, string> { { "FileId", currentFileId }, { "FileName", sendFileName } };
						returnValue.Status = status;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishFileMessageOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}

			return returnValue;
		}

		private byte[] GetByteArrayFromFilePath(string filePath)
		{
			byte[] byteArray = null;
			if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath)) {
				byteArray = System.IO.File.ReadAllBytes(filePath);
			}
			else {
				LoggingMethod.WriteToLog(pubnubLog, $"Error while reading file at {filePath}", config.LogVerbosity);
			}
			return byteArray;
		}

		private static byte[] GetMultipartFormData(byte[] sendFileByteArray, string fileName, Dictionary<string, object> formFields, string dataBoundary, CryptoModule currentCryptoModule, PNConfiguration config, IPubnubLog pubnubLog)
		{
			byte[] multipartFormData;
			string fileContentType = "application/octet-stream";
			using (Stream dataStream = new MemoryStream()) {
				foreach (var kvp in formFields) {
					if (kvp.Key == "Content-Type" && kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.ToString())) {
						fileContentType = kvp.Value.ToString();
					}
					string postParamData = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
							dataBoundary,
							kvp.Key,
							kvp.Value);
					byte[] postParam = Encoding.UTF8.GetBytes(postParamData);
					dataStream.Write(postParam, 0, postParam.Length);

					string emptyLine = "\r\n";
					byte[] emptyData = Encoding.UTF8.GetBytes(emptyLine);
					dataStream.Write(emptyData, 0, emptyData.Length);
				}
				string header = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
							dataBoundary,
							"file",
							fileName,
							fileContentType);
				byte[] postHeaderData = Encoding.UTF8.GetBytes(header);

				dataStream.Write(postHeaderData, 0, postHeaderData.Length);
				if (currentCryptoModule != null) {
					try {
						byte[] encryptBytes = currentCryptoModule.Encrypt(sendFileByteArray);
						dataStream.Write(encryptBytes, 0, encryptBytes.Length);
					} catch (Exception ex) {
						System.Diagnostics.Debug.WriteLine(ex.ToString());
					}
				} else {
					dataStream.Write(sendFileByteArray, 0, sendFileByteArray.Length);
				}

				string footer = "\r\n--" + dataBoundary + "--\r\n";
				byte[] postFooterData = Encoding.UTF8.GetBytes(footer);
				dataStream.Write(postFooterData, 0, postFooterData.Length);

				dataStream.Position = 0;
				multipartFormData = new byte[dataStream.Length];
				int bytesRead = dataStream.Read(multipartFormData, 0, multipartFormData.Length);
				System.Diagnostics.Debug.WriteLine( $"MultipartFormData created for file content, byte-count = {bytesRead}");
			}
			return multipartFormData;
		}

		private RequestParameter CreateFileUploadUrlRequestParameter()
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

		private RequestParameter CreatePublishFileMessageRequestParameter()
		{
			Dictionary<string, object> publishPayload = new Dictionary<string, object>();
			if (publishFileMessageContent != null && !string.IsNullOrEmpty(publishFileMessageContent.ToString())) {
				publishPayload.Add("message", publishFileMessageContent);
			}
			publishPayload.Add("file", new Dictionary<string, string> {
						{ "id", currentFileId },
						{ "name", sendFileName } });
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
				requestQueryStringParams.Add("ttl", ttl.ToString(CultureInfo.InvariantCulture));
			}

			if (!storeInHistory) {
				requestQueryStringParams.Add("store", "0");
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
				config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, config.UseRandomInitializationVector, pubnubLog), null);
				string encryptMessage = config.CryptoModule.Encrypt(message);
				message = jsonLibrary.SerializeToJsonString(encryptMessage);
			}
			return message;
		}
	}
}
