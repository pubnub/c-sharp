using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class StatelessSubscribeOperation<T> : Stateless_PubnubCoreBase
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

        public async Task<CancellationToken> Handshake()
        {
            CancellationTokenSource handShakeTokenSource = new CancellationTokenSource();
            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            await subscribeManager.Handshake<T>(PNOperationType.PNSubscribeOperation, channelList.ToArray(), channelgroupList.ToArray(), null, queryParam, handShakeTokenSource.Token);

            return handShakeTokenSource.Token;
        }

        public async Task<CancellationToken> ReceiveMessages(long timetoken, int region)
        {
            CancellationTokenSource receiveMessagesTokenSource = new CancellationTokenSource();

            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            await subscribeManager.ReceiveMessages<T>(PNOperationType.PNSubscribeOperation, channelList.ToArray(), channelgroupList.ToArray(), timetoken, region, false, null, queryParam, receiveMessagesTokenSource.Token);

            return receiveMessagesTokenSource.Token;
        }

        public async Task<CancellationToken> IamHere()
        {
            CancellationTokenSource iamHereTokenSource = new CancellationTokenSource();
            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            await subscribeManager.IamHere(PNOperationType.PNUnsubscribeOperation, channelList.ToArray(), channelgroupList.ToArray(), iamHereTokenSource.Token);

            return iamHereTokenSource.Token;
        }

        public async Task<CancellationToken> IamAway(string unsubChannel, string unsubChannelGroup)
        {
            CancellationTokenSource iamAwayTokenSource = new CancellationTokenSource();

            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            await subscribeManager.IAmAway<T>(PNOperationType.PNUnsubscribeOperation, unsubChannel, unsubChannelGroup, null, iamAwayTokenSource.Token);

            return iamAwayTokenSource.Token;
        }
    }

}
