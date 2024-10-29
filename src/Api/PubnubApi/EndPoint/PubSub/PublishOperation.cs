﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using PubnubApi.Security.Crypto.Cryptors;
using PubnubApi.Security.Crypto;
using System.Globalization;
using System.Collections.Concurrent;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class PublishOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private object publishContent;
		private string channelName = "";
		private bool storeInHistory = true;
		private bool httpPost;
		private Dictionary<string, object> userMetadata;
		private int ttl = -1;
		private PNCallback<PNPublishResult> savedCallback;
		private bool syncRequest;
		private Dictionary<string, object> queryParam;

		public PublishOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public PublishOperation Message(object message)
		{
			this.publishContent = message;
			return this;
		}

		public PublishOperation Channel(string channelName)
		{
			this.channelName = channelName;
			return this;
		}

		public PublishOperation ShouldStore(bool store)
		{
			this.storeInHistory = store;
			return this;
		}

		public PublishOperation Meta(Dictionary<string, object> metadata)
		{
			this.userMetadata = metadata;
			return this;
		}

		public PublishOperation UsePOST(bool post)
		{
			this.httpPost = post;
			return this;
		}

		/// <summary>
		/// ttl in hours
		/// </summary>
		/// <param name="ttl"></param>
		/// <returns></returns>
		public PublishOperation Ttl(int ttl)
		{
			this.ttl = ttl;
			return this;
		}

		public PublishOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNPublishResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNPublishResult> callback)
		{
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(channelName.Trim()) || this.publishContent == null) {
				throw new ArgumentException("Missing Channel or Message");
			}

			if (config == null || string.IsNullOrEmpty(config.PublishKey) || config.PublishKey.Trim().Length <= 0) {
				throw new MissingMemberException("publish key is required");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}
			this.savedCallback = callback;
			Publish(channelName, this.publishContent, this.storeInHistory, this.ttl, this.userMetadata, this.queryParam, callback);
		}

		public async Task<PNResult<PNPublishResult>> ExecuteAsync()
		{
			syncRequest = false;
			return await Publish(this.channelName, this.publishContent, this.storeInHistory, this.ttl, this.userMetadata, this.queryParam).ConfigureAwait(false);
		}

		public PNPublishResult Sync()
		{
			if (this.publishContent == null) {
				throw new ArgumentException("message cannot be null");
			}

			if (config == null || string.IsNullOrEmpty(config.PublishKey) || config.PublishKey.Trim().Length <= 0) {
				throw new MissingMemberException("publish key is required");
			}

			ManualResetEvent syncEvent = new System.Threading.ManualResetEvent(false);
			Task<PNPublishResult> task = Task<PNPublishResult>.Factory.StartNew(() => {
				syncRequest = true;
				syncEvent = new System.Threading.ManualResetEvent(false);
				Publish(this.channelName, this.publishContent, this.storeInHistory, this.ttl, this.userMetadata, this.queryParam, new PNPublishResultExt((r, s) => { SyncResult = r; syncEvent.Set(); }));
				syncEvent.WaitOne(config.NonSubscribeRequestTimeout * 1000);

				return SyncResult;
			}, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
			return task.Result;
		}

		private static PNPublishResult SyncResult { get; set; }

		internal void Retry()
		{
			Publish(this.channelName, this.publishContent, this.storeInHistory, this.ttl, this.userMetadata, this.queryParam, savedCallback);
		}

		internal void Publish(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> userMetadata, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message"));
				callback.OnResponse(null, status);
				return;
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				PNStatus status = new PNStatus();
				status.Error = true;
				status.ErrorData = new PNErrorData("Invalid publish key", new MissingMemberException("Invalid publish key"));
				callback.OnResponse(null, status);
				return;
			}
			RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNPublishOperation;
			requestState.PubnubCallback = callback;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			var requestParameters = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameters, operationType: PNOperationType.PNPublishOperation);

			PubnubInstance.transportMiddleware.Send(transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse(requestState, responseString);
						if (result != null && result.Count >= 3) {
							_ = int.TryParse(result[0].ToString(), out var publishStatus);
							if (publishStatus == 1) {
								ProcessResponseCallbacks(result, requestState);
							} else {
								PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
								PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNPublishOperation, category, requestState, 400, new PNException(responseString));
								if (requestState.PubnubCallback != null) {
									requestState.PubnubCallback.OnResponse(default, status);
								}
							}
						} else {
							ProcessResponseCallbacks(result, requestState);
						}
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
			CleanUp();
		}

		internal async Task<PNResult<PNPublishResult>> Publish(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNPublishResult> returnValue = new PNResult<PNPublishResult>();

			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
				PNStatus errStatus = new PNStatus
				{
					Error = true,
					ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message"))
				};
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				PNStatus errStatus = new PNStatus();
				errStatus.Error = true;
				errStatus.ErrorData = new PNErrorData("Invalid publish key", new MissingMemberException("Invalid publish key"));
				returnValue.Status = errStatus;
				return returnValue;
			}
			RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			requestState.Channels = new[] { channel };
			requestState.ResponseType = PNOperationType.PNPublishOperation;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNPublishOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, Constants.HttpRequestSuccessStatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;

				if (!string.IsNullOrEmpty(json)) {
					List<object> result = ProcessJsonResponse(requestState, json);

					if (result is { Count: >= 3 }) {
						_ = int.TryParse(result[0].ToString(), out var publishStatus);
						if (publishStatus == 1) {
							List<object> resultList = ProcessJsonResponse(requestState, json);
							if (resultList is { Count: > 0 }) {
								ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
								PNPublishResult responseResult = responseBuilder.JsonToObject<PNPublishResult>(resultList, true);
								if (responseResult != null) {
									returnValue.Result = responseResult;
								}
							}
						}
					}
				}
			} else {
				var statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNPublishOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			return returnValue;
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
			PubnubInstance = instance;

			if (!ChannelRequest.ContainsKey(instance.InstanceId)) {
				ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
			}
			if (!ChannelInternetStatus.ContainsKey(instance.InstanceId)) {
				ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
			}
			if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId)) {
				ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
			}
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

		private RequestParameter CreateRequestParameter()
		{
			List<string> urlSegments = new List<string>
			{
				"publish",
				config.PublishKey?? "",
				config.SubscribeKey??"",
				"0",
				channelName,
				"0"
			};
			if (!httpPost) {
				urlSegments.Add(PrepareContent(this.publishContent));
			}
			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (userMetadata != null) {
				string jsonMetaData = jsonLibrary.SerializeToJsonString(userMetadata);
				requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(jsonMetaData, PNOperationType.PNPublishOperation, false, false, false));
			}

			if (storeInHistory && ttl >= 0) {
				requestQueryStringParams.Add("tt1", ttl.ToString(CultureInfo.InvariantCulture));
			}

			if (!storeInHistory) {
				requestQueryStringParams.Add("store", "0");
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNPublishOperation, false, false, false));
					}
				}
			}

			var requestParam = new RequestParameter() {
				RequestType = httpPost ? Constants.POST : Constants.GET,
				PathSegment = urlSegments,
				Query = requestQueryStringParams
			};
			if (httpPost) {
				string postMessage = PrepareContent(publishContent);
				requestParam.BodyContentString = postMessage;
			}
			return requestParam;
		}

		private void CleanUp()
		{
			savedCallback = null;
		}
	}
}
