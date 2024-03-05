using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class HeartbeatOperation : PubnubCoreBase
	{
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly TelemetryManager pubnubTelemetryMgr;
        private Dictionary<string, object> userState;

        public HeartbeatOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

            PubnubInstance = instance;
        }      

        internal async Task<PNStatus> HeartbeatRequest<T>(string[] channels, string[] channelGroups) {
            PNStatus resp = null;
            try
            {
                //string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);    // TODO: State handling through configuration flag

                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, "");
                Uri request = urlBuilder.BuildPresenceHeartbeatRequest("GET", "", channels, channelGroups, "");
                RequestState<T> pubnubRequestState = new RequestState<T>();
                pubnubRequestState.Channels = channels;
                pubnubRequestState.ChannelGroups = channelGroups;
                pubnubRequestState.ResponseType = PNOperationType.PNHeartbeatOperation;
                pubnubRequestState.TimeQueued = DateTime.Now;
                
                await UrlProcessRequest<T>(request, pubnubRequestState, false).ContinueWith(r =>
                {
                    resp = r.Result.Item2;
                }, TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} HeartbeatOperation=> HeartbeatRequest \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), string.Join(",", channelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
                return new PNStatus(ex, PNOperationType.PNHeartbeatOperation, PNStatusCategory.PNUnknownCategory, channels, channelGroups);
            }
            return resp;
        }

	}
}
