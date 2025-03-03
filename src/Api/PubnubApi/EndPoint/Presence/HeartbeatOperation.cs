using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class HeartbeatOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;

		public HeartbeatOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
			PubnubInstance = instance;
		}

		internal async Task<PNStatus> HeartbeatRequest<T>(string[] channels, string[] channelGroups)
		{
			PNStatus responseStatus = null;
			try {
				string presenceState = string.Empty;

				if (config.MaintainPresenceState) presenceState = BuildJsonUserState(channels, channelGroups, true);

				RequestState<T> pubnubRequestState = new RequestState<T>();
				pubnubRequestState.Channels = channels;
				pubnubRequestState.ChannelGroups = channelGroups;
				pubnubRequestState.ResponseType = PNOperationType.PNHeartbeatOperation;
				pubnubRequestState.TimeQueued = DateTime.Now;

				var requestParameter = CreateRequestParameter(channels: channels, channelGroups: channelGroups);
				var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNHeartbeatOperation);
				var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					pubnubRequestState.GotJsonResponse = true;
					PNStatus errorStatus = GetStatusIfError(pubnubRequestState, responseString);
					if (errorStatus == null) {
						responseStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(pubnubRequestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, pubnubRequestState, (int)HttpStatusCode.OK, null);
					} else {
						responseStatus = errorStatus;
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					responseStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNHeartbeatOperation, category, pubnubRequestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				}
			} catch (Exception ex) {
				logger?.Error($"Heartbeat request for channel(s)={string.Join(", ", channels.OrderBy(x => x))} \n channelGroup(s)={string.Join(", ", channelGroups.OrderBy(x => x))} \n Exception Details={ex}");
				return new PNStatus(ex, PNOperationType.PNHeartbeatOperation, PNStatusCategory.PNUnknownCategory, channels, channelGroups);
			}
			return responseStatus;
		}

		private RequestParameter CreateRequestParameter(string[] channels, string[] channelGroups)
		{
			string channelString = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>
			{
				"v2",
				"presence",
				"sub_key",
				config.SubscribeKey,
				"channel",
				channelString,
				"heartbeat"
			};
			string presenceState = string.Empty;

			if (config.MaintainPresenceState) presenceState = BuildJsonUserState(channels, channelGroups, true);
			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			string channelsJsonState = presenceState;
			if (channelsJsonState != "{}" && channelsJsonState != string.Empty) {
				requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(channelsJsonState, PNOperationType.PNHeartbeatOperation, false, false, false));
			}

			if (channelGroups != null && channelGroups.Length > 0) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), PNOperationType.PNHeartbeatOperation, false, false, false));
			}

			if (config.PresenceTimeout != 0) {
				requestQueryStringParams.Add("heartbeat", config.PresenceTimeout.ToString(CultureInfo.InvariantCulture));
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
