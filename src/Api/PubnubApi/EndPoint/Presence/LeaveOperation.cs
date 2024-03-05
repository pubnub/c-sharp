using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class LeaveOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;
		private readonly TelemetryManager pubnubTelemetryManager;

		public LeaveOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
			pubnubTelemetryManager = telemetryManager;
			PubnubInstance = instance;
		}

		internal async Task<PNStatus> LeaveRequest<T>(string[] channels, string[] channelGroups)
		{
			PNStatus resp = null;
			try {
				IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryManager, null, string.Empty);
				Uri request = urlBuilder.BuildMultiChannelLeaveRequest("GET", string.Empty, channels, channelGroups, string.Empty, null);
				RequestState<T> pubnubRequestState = new RequestState<T>();
				pubnubRequestState.Channels = channels;
				pubnubRequestState.ChannelGroups = channelGroups;
				pubnubRequestState.ResponseType = PNOperationType.Leave;
				pubnubRequestState.TimeQueued = DateTime.Now;
				await UrlProcessRequest<T>(request, pubnubRequestState, false).ContinueWith(r => {
					resp = r.Result.Item2;
				}, TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} Leave=> LeaveRequest \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), string.Join(",", channelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
				return new PNStatus(ex, PNOperationType.Leave, PNStatusCategory.PNUnknownCategory, channels, channelGroups);
			}
			return resp;
		}
	}
}

