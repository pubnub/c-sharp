﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	public class GetStateOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;


		private string[] channelNames;
		private string[] channelGroupNames;
		private string channelUUID = "";
		private PNCallback<PNGetStateResult> savedCallback;
		private Dictionary<string, object> queryParam;

		public GetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;

		}

		public GetStateOperation Channels(string[] channels)
		{
			this.channelNames = channels;
			return this;
		}

		public GetStateOperation ChannelGroups(string[] channelGroups)
		{
			this.channelGroupNames = channelGroups;
			return this;
		}

		public GetStateOperation Uuid(string uuid)
		{
			this.channelUUID = uuid;
			return this;
		}

		public GetStateOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		[Obsolete("Async is deprecated, please use Execute instead.")]
		public void Async(PNCallback<PNGetStateResult> callback)
		{
			Execute(callback);
		}

		public void Execute(PNCallback<PNGetStateResult> callback)
		{
			this.savedCallback = callback;
			logger?.Trace($"{GetType().Name} Execute invoked");
			GetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, this.queryParam, callback);
		}

		public async Task<PNResult<PNGetStateResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await GetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, this.queryParam).ConfigureAwait(false);
		}

		internal void Retry()
		{
			GetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, this.queryParam, savedCallback);
		}

		internal void GetUserState(string[] channels, string[] channelGroups, string uuid, Dictionary<string, object> externalQueryParam, PNCallback<PNGetStateResult> callback)
		{
			if ((channels == null && channelGroups == null)
						   || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0)) {
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided");
			}
			string internalUuid;
			if (string.IsNullOrEmpty(uuid) || uuid.Trim().Length == 0) {
				internalUuid = config.UserId;
			} else {
				internalUuid = uuid;
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNGetStateResult> requestState = new RequestState<PNGetStateResult>
			{
				Channels = channels,
				ChannelGroups = channelGroups,
				ResponseType = PNOperationType.PNGetStateOperation,
				PubnubCallback = callback,
				Reconnect = false,
				EndPointOperation = this
			};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetStateOperation);
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
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
						callback.OnResponse(default, errorStatus);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetStateOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
		}

		internal async Task<PNResult<PNGetStateResult>> GetUserState(string[] channels, string[] channelGroups, string uuid, Dictionary<string, object> externalQueryParam)
		{
			if ((channels == null && channelGroups == null)
						   || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0)) {
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided");
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			PNResult<PNGetStateResult> returnValue = new PNResult<PNGetStateResult>();

			string internalUuid;
			if (string.IsNullOrEmpty(uuid) || uuid.Trim().Length == 0) {
				internalUuid = config.UserId;
			} else {
				internalUuid = uuid;
			}
			RequestState<PNGetStateResult> requestState = new RequestState<PNGetStateResult>
			{
				Channels = channels,
				ChannelGroups = channelGroups,
				ResponseType = PNOperationType.PNGetStateOperation,
				Reconnect = false,
				EndPointOperation = this
			};
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNGetStateOperation);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				Tuple<string, PNStatus> jsonAndStatusTuple;
				if (errorStatus == null) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
					jsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					jsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
				returnValue.Status = jsonAndStatusTuple.Item2;
				string json = jsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
					PNGetStateResult responseResult = responseBuilder.JsonToObject<PNGetStateResult>(resultList, true);
					if (responseResult != null) {
						returnValue.Result = responseResult;
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNGetStateOperation, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
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
			string requestUuid = string.IsNullOrEmpty(channelUUID) || channelUUID.Trim().Length == 0 ? config.UserId : channelUUID;

			string channelsCommaDelimited = (channelNames != null && channelNames.Length > 0) ? string.Join(",", channelNames.OrderBy(x => x).ToArray()) : string.Empty;
			string channelGroupsCommaDelimited = (channelGroupNames != null && channelGroupNames.Length > 0) ? string.Join(",", channelGroupNames.OrderBy(x => x).ToArray()) : string.Empty;

			List<string> pathSegments = new List<string>
			{
				"v2",
				"presence",
				"sub_key",
				config.SubscribeKey,
				"channel",
				string.IsNullOrEmpty(channelsCommaDelimited) || channelsCommaDelimited.Trim().Length <= 0 ? "," : channelsCommaDelimited,
				"uuid",
				requestUuid
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroupsCommaDelimited, PNOperationType.PNGetStateOperation, false, false, false));
			}

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNGetStateOperation, false, false, false));
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
