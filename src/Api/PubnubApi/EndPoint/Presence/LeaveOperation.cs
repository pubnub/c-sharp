using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class LeaveOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;

		public LeaveOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			PubnubInstance = instance;
		}

		internal async Task<PNStatus> LeaveRequest<T>(string[] channels, string[] channelGroups)
		{
			PNStatus responseStatus = null;
			try {

				RequestState<T> pubnubRequestState = new RequestState<T>();
				pubnubRequestState.Channels = channels;
				pubnubRequestState.ChannelGroups = channelGroups;
				pubnubRequestState.ResponseType = PNOperationType.Leave;
				pubnubRequestState.TimeQueued = DateTime.Now;

				var requestParameter = CreateRequestParameter(channels: channels, channelGroups: channelGroups);
				var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.Leave);
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
					responseStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.Leave, category, pubnubRequestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				}
			} catch (Exception ex)
			{
				logger?.Error(
					$"Presence Leave request for channel(s)={string.Join(", ", channels.OrderBy(x => x))} \n channelGroup(s)={string.Join(", ", channelGroups.OrderBy(x => x))} \n Exception Details={ex}");
				return new PNStatus(ex, PNOperationType.Leave, PNStatusCategory.PNUnknownCategory, channels, channelGroups);
			}
			logger?.Info($"{GetType().Name} request finished with status code {responseStatus.StatusCode}");
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
				"leave"
			};

			var requestQueryStringParams = new Dictionary<string, string>();

			if (channelGroups != null && channelGroups.Length > 0) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), PNOperationType.Leave, false, false, false));
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

