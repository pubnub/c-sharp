using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class FireOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		private object publishContent;
		private string channelName = "";
		private bool httpPost;
		private Dictionary<string, object> userMetadata;
		private readonly int ttl = -1;
		private PNCallback<PNPublishResult> savedCallback;
		private bool syncRequest;
		private Dictionary<string, object> queryParam;

		public FireOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}

		public FireOperation Message(object message)
		{
			this.publishContent = message;
			return this;
		}

		public FireOperation Channel(string channelName)
		{
			this.channelName = channelName;
			return this;
		}

		public FireOperation Meta(Dictionary<string, object> metadata)
		{
			this.userMetadata = metadata;
			return this;
		}

		public FireOperation UsePOST(bool post)
		{
			this.httpPost = post;
			return this;
		}

		public FireOperation QueryParam(Dictionary<string, object> customQueryParam)
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
			if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim()) || this.publishContent == null) {
				throw new ArgumentException("Missing Channel or Message");
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				throw new MissingMemberException("Invalid publish key");
			}

			if (callback == null) {
				throw new ArgumentException("Missing userCallback");
			}
			logger?.Trace($"{GetType().Name} Execute invoked");
			Fire(this.channelName, this.publishContent, false, this.ttl, this.userMetadata, this.queryParam, callback);
		}

		public async Task<PNResult<PNPublishResult>> ExecuteAsync()
		{
			syncRequest = false;
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await Fire(this.channelName, this.publishContent, false, this.ttl, this.userMetadata, this.queryParam).ConfigureAwait(false);
		}

		public PNPublishResult Sync()
		{
			System.Threading.ManualResetEvent syncEvent = new System.Threading.ManualResetEvent(false);
			syncRequest = true;
			syncEvent = new System.Threading.ManualResetEvent(false);
			Fire(this.channelName, this.publishContent, false, this.ttl, this.userMetadata, this.queryParam, new PNPublishResultExt((r, s) => { SyncResult = r; syncEvent.Set(); }));
			syncEvent.WaitOne(config.NonSubscribeRequestTimeout * 1000);
			return SyncResult;
		}

		private static PNPublishResult SyncResult { get; set; }

		internal void Retry()
		{
			Fire(this.channelName, this.publishContent, false, this.ttl, this.userMetadata, this.queryParam, savedCallback);
		}

		private void Fire(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
		{
			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
				PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message")) };
				callback.OnResponse(null, status);
				return;
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
				callback.OnResponse(null, status);
				return;
			}

			if (callback == null) {
				return;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>
			{
				Channels = new[] { channel },
				ResponseType = PNOperationType.PNFireOperation,
				PubnubCallback = callback,
				Reconnect = false,
				EndPointOperation = this
			};

			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNFireOperation);

			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					if (!string.IsNullOrEmpty(responseString)) {
                        requestState.GotJsonResponse = true;
						List<object> result = ProcessJsonResponse<PNPublishResult>(requestState, responseString);
						if (result != null && result.Count >= 3) {
							int publishStatus;
							var _ = Int32.TryParse(result[0].ToString(), out publishStatus);
							if (publishStatus == 1) {
								logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
								ProcessResponseCallbacks(result, requestState);
							} else {
								PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
								PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNFireOperation, category, requestState, 400, new PNException(responseString));
								if (requestState.PubnubCallback != null) {
									logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
									requestState.PubnubCallback.OnResponse(default(PNPublishResult), status);
								}
							}
						} else {
							logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
							ProcessResponseCallbacks(result, requestState);
						}
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNFireOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response.StatusCode}");
					requestState.PubnubCallback.OnResponse(default(PNPublishResult), status);
				}
			});
		}

		private async Task<PNResult<PNPublishResult>> Fire(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam)
		{
			PNResult<PNPublishResult> returnValue = new PNResult<PNPublishResult>();

			if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message")) };
				returnValue.Status = errStatus;
				return returnValue;
			}

			if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0) {
				PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
				returnValue.Status = errStatus;
				return returnValue;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>
			{
				Channels = new[] { channel },
				ResponseType = PNOperationType.PNFireOperation,
				Reconnect = false,
				EndPointOperation = this
			};

			Tuple<string, PNStatus> JsonAndStatusTuple;
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNFireOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);

			if (transportResponse.Error == null) {
				string responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> result = ProcessJsonResponse(requestState, json);
					if (result != null && result.Count >= 3) {
						int publishStatus;
						_ = int.TryParse(result[0].ToString(), out publishStatus);
						if (publishStatus == 1) {
							List<object> resultList = ProcessJsonResponse(requestState, json);
							if (resultList != null && resultList.Count > 0) {
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
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNFireOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
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
			requestQueryStringParams.Add("norep", "true");

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
		private string PrepareContent(object originalMessage)
		{
			string message = jsonLibrary.SerializeToJsonString(originalMessage);
			if (config.CryptoModule != null || config.CipherKey.Length > 0) {
				config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, config.UseRandomInitializationVector), null);
				string encryptMessage = config.CryptoModule.Encrypt(message);
				message = jsonLibrary.SerializeToJsonString(encryptMessage);
			}
			return message;
		}
	}

}
