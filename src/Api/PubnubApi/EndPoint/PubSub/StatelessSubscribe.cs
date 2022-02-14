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

    public class SubscribeStateMachine
    {
        public enum SubscribeState
        {
            HandshakeFailed,
            Unsubscribed,
            ReconnectionFailed,
            Handshaking,
            Preparing,
            Stopped,
            Reconnecting,
            Receiving,
            Delivering
        }

        public enum SubscribeEvent
        {
            Reconnect,
            SubscriptionChange,
            Fail,
            Giveup,
            Success,
            Subscribe,
            Restore,
            Disconnect,
            Done
        }

        public SubscribeState ChangeSubscribeState(SubscribeState current, SubscribeEvent subscribeEvt)
        {
            SubscribeState ret;
            switch (current)
            {
                case SubscribeState.Handshaking:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.SubscriptionChange:
                        case SubscribeEvent.Fail:
                            ret = SubscribeState.Handshaking;
                            break;
                        case SubscribeEvent.Success:
                            ret = SubscribeState.Receiving;
                            break;
                        case SubscribeEvent.Giveup:
                            ret = SubscribeState.HandshakeFailed;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.HandshakeFailed:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.SubscriptionChange:
                        case SubscribeEvent.Reconnect:
                            ret = SubscribeState.Handshaking;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.Reconnecting:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.SubscriptionChange:
                        case SubscribeEvent.Fail:
                            ret = SubscribeState.Reconnecting;
                            break;
                        case SubscribeEvent.Success:
                            ret = SubscribeState.Delivering;
                            break;
                        case SubscribeEvent.Giveup:
                            ret = SubscribeState.ReconnectionFailed;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.ReconnectionFailed:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.SubscriptionChange:
                        case SubscribeEvent.Reconnect:
                            ret = SubscribeState.Reconnecting;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.Unsubscribed:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.Subscribe:
                            ret = SubscribeState.Handshaking;
                            break;
                        case SubscribeEvent.Restore:
                            ret = SubscribeState.Reconnecting;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.Preparing:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.Reconnect:
                            ret = SubscribeState.Handshaking;
                            break;
                        case SubscribeEvent.SubscriptionChange:
                            ret = SubscribeState.Preparing;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.Stopped:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.Reconnect:
                            ret = SubscribeState.Reconnecting;
                            break;
                        case SubscribeEvent.SubscriptionChange:
                            ret = SubscribeState.Stopped;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.Receiving:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.Success:
                            ret = SubscribeState.Delivering;
                            break;
                        case SubscribeEvent.Fail:
                            ret = SubscribeState.Reconnecting;
                            break;
                        case SubscribeEvent.SubscriptionChange:
                            ret = SubscribeState.Receiving;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case SubscribeState.Delivering:
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.Done:
                            ret = SubscribeState.Receiving;
                            break;
                        default:
                            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                default:
                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
            }

            return ret;

        }

    }
}
