using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class StatelessSubscribeOperation<T> : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        List<string> channelList = null;
        List<string> channelgroupList = null;
        private Dictionary<string, object> queryParam;

        public StatelessSubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;
        }

        public StatelessSubscribeOperation<T> Channels(List<string> channels)
        {
            channelList = channels;
            return this;
        }

        public StatelessSubscribeOperation<T> ChannelGroups(List<string> channelGroups)
        {
            channelgroupList = channelGroups;
            return this;
        }

        public StatelessSubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public async Task<CancellationTokenSource> Handshake()
        {
            CancellationTokenSource handShakeTokenSource = new CancellationTokenSource();
            SubscribeManager subscribeManager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            subscribeManager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channelList.ToArray(), channelgroupList.ToArray(), null, queryParam);
#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
            await Task.Delay(1); //Added this until this method gets await stmt.
#endif

            return handShakeTokenSource;
        }

        public async Task<CancellationTokenSource> ReceiveMessages(long timetoken, int region)
        {
            CancellationTokenSource receiveMessagesTokenSource = new CancellationTokenSource();

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("tt", timetoken.ToString());
            additionalParams.Add("r", region.ToString());

            SubscribeManager subscribeManager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            subscribeManager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channelList.ToArray(), channelgroupList.ToArray(), additionalParams, queryParam);

#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
                await Task.Delay(30 * 1000); //Added this until this method gets await stmt.
#endif

            return receiveMessagesTokenSource;
        }

        public async Task<CancellationTokenSource> IamHere()
        {
            CancellationTokenSource iamHereTokenSource = new CancellationTokenSource();

            //Cannot route to Register PresenceHeartbeat without code refactoring.

#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
                await Task.Delay(30 * 1000); //Added this until this method gets await stmt.
#endif

            return iamHereTokenSource;
        }

        public async Task<CancellationTokenSource> IamAway(string unsubChannel, string unsubChannelGroup)
        {
            CancellationTokenSource iamAwayTokenSource = new CancellationTokenSource();

            SubscribeManager subscribeManager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            subscribeManager.MultiChannelUnSubscribeInit<T>(PNOperationType.PNUnsubscribeOperation, unsubChannel, unsubChannelGroup, null);

#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
                await Task.Delay(30 * 1000); //Added this until this method gets await stmt.
#endif

            return iamAwayTokenSource;
        }
    }

}
