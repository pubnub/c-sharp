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
		private readonly IPubnubLog pubnubLog;

		public LeaveOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
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
				var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
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
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} HeartbeatOperation=> HeartbeatRequest \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), string.Join(",", channelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
				return new PNStatus(ex, PNOperationType.Leave, PNStatusCategory.PNUnknownCategory, channels, channelGroups);
			}
			return responseStatus;
		}
		private RequestParameter CreateRequestParameter(string[] channels, string[] channelGroups)
		{
			string channleString = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>
			{
				"v2",
				"presence",
				"sub_key",
				config.SubscribeKey,
				"channel",
				channleString,
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

