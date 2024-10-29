﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi.EndPoint
{
	public class DownloadFileOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private PNCallback<PNDownloadFileResult> savedCallback;
		private Dictionary<string, object> queryParam;

		private string channelName;
		private string currentFileId;
		private string currentFileName;
		private string currentFileCipherKey;

		public DownloadFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public DownloadFileOperation Channel(string channel)
		{
			channelName = channel;
			return this;
		}

		public DownloadFileOperation FileId(string fileId)
		{
			currentFileId = fileId;
			return this;
		}

		public DownloadFileOperation FileName(string fileName)
		{
			currentFileName = fileName;
			return this;
		}

		public DownloadFileOperation CipherKey(string cipherKeyForFile)
		{
			currentFileCipherKey = cipherKeyForFile;
			return this;
		}

		public DownloadFileOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNDownloadFileResult> callback)
		{
			if (string.IsNullOrEmpty(currentFileId)) {
				throw new ArgumentException("Missing File Id");
			}
			if (string.IsNullOrEmpty(currentFileName)) {
				throw new ArgumentException("Missing File Name");
			}

			savedCallback = callback ?? throw new ArgumentException("Missing callback");
			ProcessFileDownloadRequest(savedCallback);
		}

		public async Task<PNResult<PNDownloadFileResult>> ExecuteAsync()
		{
			return await ProcessFileDownloadRequest().ConfigureAwait(false);
		}

		internal void Retry()
		{
			ProcessFileDownloadRequest(savedCallback);
		}

		private void ProcessFileDownloadRequest(PNCallback<PNDownloadFileResult> callback)
		{
			RequestState<PNDownloadFileResult> requestState = new RequestState<PNDownloadFileResult>();
			requestState.ResponseType = PNOperationType.PNDownloadFileOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDownloadFileOperation);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null)
				{
					var fileContentBytes = transportResponse.Content;
					if (fileContentBytes != null) {
						requestState.GotJsonResponse = true;
						byte[] outputBytes;
						if (string.IsNullOrEmpty(currentFileCipherKey) && string.IsNullOrEmpty(config.CipherKey) && config.CryptoModule == null) {
							outputBytes = fileContentBytes;
						} else {
							CryptoModule currentCryptoModule = !string.IsNullOrEmpty(currentFileCipherKey) ? new CryptoModule(new LegacyCryptor(currentFileCipherKey, true, pubnubLog), null) : (config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, true, pubnubLog), null));
							try {
								outputBytes = currentCryptoModule.Decrypt(fileContentBytes);
								LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Stream length (after Decrypt)= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), fileContentBytes.Length), config.LogVerbosity);
							} catch (Exception ex) {
								System.Diagnostics.Debug.WriteLine("{0}\nMessage might be not encrypted, returning as is...", ex.ToString());
								outputBytes = fileContentBytes;
							}
						}
						PNDownloadFileResult result = new PNDownloadFileResult
						{
							FileBytes = outputBytes,
							FileName = currentFileName
						};
						PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 200, null);
						callback.OnResponse(result, status);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, null);
						callback.OnResponse(default, errorStatus);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDownloadFileOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}
		private async Task<PNResult<PNDownloadFileResult>> ProcessFileDownloadRequest()
		{
			PNResult<PNDownloadFileResult> returnValue = new PNResult<PNDownloadFileResult>();

			if (string.IsNullOrEmpty(currentFileId)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File Id", new ArgumentException("Missing File Id")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(currentFileName)) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			
			RequestState<PNDownloadFileResult> requestState = new RequestState<PNDownloadFileResult>
				{
					ResponseType = PNOperationType.PNDownloadFileOperation,
					Reconnect = false,
					EndPointOperation = this
				};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNDownloadFileOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null)
			{
				var fileContentBytes = transportResponse.Content;
				if (fileContentBytes != null) {
					byte[] outputBytes;
					requestState.GotJsonResponse = true;
					if (string.IsNullOrEmpty(currentFileCipherKey) && string.IsNullOrEmpty(config.CipherKey) && config.CryptoModule == null) {
						outputBytes = fileContentBytes;
					} else {
						CryptoModule currentCryptoModule = !string.IsNullOrEmpty(currentFileCipherKey) ? new CryptoModule(new LegacyCryptor(currentFileCipherKey, true, pubnubLog), null) : (config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, true, pubnubLog), null));
						try {
							outputBytes = currentCryptoModule.Decrypt(fileContentBytes);
							LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Stream length (after Decrypt)= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), fileContentBytes.Length), config.LogVerbosity);
						} catch (Exception ex) {
							System.Diagnostics.Debug.WriteLine("{0}\nFile content might be not encrypted, returning as is...", ex.ToString());
							outputBytes = fileContentBytes;
							returnValue.Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Decryption error", ex) };
						}
					}
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					PNDownloadFileResult result = new PNDownloadFileResult
					{
						FileBytes = outputBytes,
						FileName = currentFileName
					};
					returnValue.Result = result;
					returnValue.Status = status;
				} else {
					PNStatus errorStatus = GetStatusIfError(requestState, null);
					returnValue.Status = errorStatus;
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNDownloadFileOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		private RequestParameter CreateRequestParameter()
		{
			var pathSegments = new List<string>
			{
				"v1",
				"files",
				config.SubscribeKey,
				"channels",
				channelName,
				"files",
				currentFileId,
				currentFileName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNDownloadFileOperation, false, false, false));
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
