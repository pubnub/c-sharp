using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stateless;

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

            subscribeCall.Configure(SubscribeState.Stopped)
                .Permit(SubscribeEvent.Subscribe, SubscribeState.Handshaking);

            subscribeCall.Configure(SubscribeState.Handshaking)
                .OnEntry(async () => await OnEntryHandshaking())
                .Permit(SubscribeEvent.HandshakeSuccess, SubscribeState.Receiving)
                .Permit(SubscribeEvent.Giveup, SubscribeState.HandshakeFailed)
                .PermitReentry(SubscribeEvent.SubscriptionChange)
                .PermitReentry(SubscribeEvent.Fail);


            subscribeCall.Configure(SubscribeState.HandshakeFailed)
                //.OnEntry(async () => await Handshake())
                .Permit(SubscribeEvent.SubscriptionChange, SubscribeState.Handshaking)
                .Permit(SubscribeEvent.Reconnect, SubscribeState.Handshaking);

            subscribeCall.Configure(SubscribeState.Reconnecting)
                //.OnEntry(async () => await Handshake())
                .Permit(SubscribeEvent.ReceiveSuccess, SubscribeState.Delivering)
                .Permit(SubscribeEvent.Giveup, SubscribeState.ReconnectionFailed)
                .PermitReentry(SubscribeEvent.SubscriptionChange)
                .PermitReentry(SubscribeEvent.Fail);

            subscribeCall.Configure(SubscribeState.ReconnectionFailed)
                //.OnEntry(async () => await Handshake())
                .Permit(SubscribeEvent.SubscriptionChange, SubscribeState.Reconnecting)
                .Permit(SubscribeEvent.Reconnect, SubscribeState.Reconnecting);

            subscribeCall.Configure(SubscribeState.Unsubscribed)
                .OnEntry(async()=> await IamAway(null, null))
                .Permit(SubscribeEvent.Subscribe, SubscribeState.Handshaking)
                .Permit(SubscribeEvent.Restore, SubscribeState.Reconnecting);

            subscribeCall.Configure(SubscribeState.Preparing)
                .Permit(SubscribeEvent.Reconnect, SubscribeState.Handshaking)
                .PermitReentry(SubscribeEvent.SubscriptionChange);

            subscribeCall.Configure(SubscribeState.Stopped)
                .Permit(SubscribeEvent.Reconnect, SubscribeState.Reconnecting)
                .PermitReentry(SubscribeEvent.SubscriptionChange);

            var receiveSubscribeSuccessEvent = subscribeCall.SetTriggerParameters<long, int>(SubscribeEvent.ReceiveSuccess);
            subscribeCall.Configure(SubscribeState.Receiving)
                .OnEntry(async() => await OnEntryReceiving(0, 0))
                .Permit(SubscribeEvent.ReceiveSuccess, SubscribeState.Delivering)
                .Permit(SubscribeEvent.Fail, SubscribeState.Reconnecting)
                .PermitReentry(SubscribeEvent.SubscriptionChange);

            subscribeCall.Configure(SubscribeState.Delivering)
                .OnEntry(() => System.Diagnostics.Debug.WriteLine("Message delivered logic"))
                .Permit(SubscribeEvent.Done, SubscribeState.Receiving);
        }

        internal async Task<CancellationToken> OnEntryHandshaking()
        {
            CancellationTokenSource handShakeTokenSource = new CancellationTokenSource();
            Tuple<string, PNStatus>  handshakeResponse = await Handshake(handShakeTokenSource.Token);
            PNStatus status = handshakeResponse.Item2;
            if (!status.Error && status.StatusCode == 200)
            {
                subscribeCall.Fire(SubscribeEvent.HandshakeSuccess);
            }
            return handShakeTokenSource.Token;
        }

        internal async Task<CancellationToken> OnEntryReceiving(long timetoken, int region)
        {
            CancellationTokenSource receivingTokenSource = new CancellationTokenSource();
            Tuple<string, PNStatus> handshakeResponse = await ReceiveMessages(timetoken, region, receivingTokenSource.Token);
            PNStatus status = handshakeResponse.Item2;
            if (!status.Error && status.StatusCode == 200)
            {
                long currentTimetoken = 0; //TBD
                int currentRegion = 0; //TBD
               
                subscribeCall.Fire(SubscribeEvent.ReceiveSuccess); //TBD - How to pass parameters to Fire
            }
            return receivingTokenSource.Token;

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

        private async Task<Tuple<string, PNStatus>> Handshake(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("Handshake");
            
            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            Tuple<string, PNStatus> handshakeResponse = await subscribeManager.Handshake<T>(PNOperationType.PNSubscribeOperation, channelList.ToArray(), (channelgroupList != null) ? channelgroupList.ToArray() : null, null, queryParam, cancellationToken);

            System.Diagnostics.Debug.WriteLine(handshakeResponse.Item1);
            System.Diagnostics.Debug.WriteLine(jsonLibrary.SerializeToJsonString(handshakeResponse.Item2));
            
            return handshakeResponse;
        }

        public async Task<Tuple<string, PNStatus>> ReceiveMessages(long timetoken, int region, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("ReceiveMessages");
            
            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, base.PubnubInstance);
            Tuple<string, PNStatus> receiveMessageResponse = await subscribeManager.ReceiveMessages<T>(PNOperationType.PNSubscribeOperation, channelList.ToArray(), (channelgroupList != null) ? channelgroupList.ToArray() : null, timetoken, region, false, null, queryParam, cancellationToken);
            
            System.Diagnostics.Debug.WriteLine(receiveMessageResponse.Item1);
            System.Diagnostics.Debug.WriteLine(jsonLibrary.SerializeToJsonString(receiveMessageResponse.Item2));

            return receiveMessageResponse;
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

        StateMachine<SubscribeState, SubscribeEvent> subscribeCall = new StateMachine<SubscribeState, SubscribeEvent>(SubscribeState.Stopped);
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
            HandshakeSuccess,
            ReceiveSuccess,
            ReconnectSuccess,
            Subscribe,
            Restore,
            Disconnect,
            Done
        }

        public void Execute()
        {
            string statePicture = Stateless.Graph.UmlDotGraph.Format(subscribeCall.GetInfo());
            System.Diagnostics.Debug.WriteLine(statePicture);
            subscribeCall.Fire(SubscribeEvent.Subscribe);
        }

        private void Reconnect()
        {
            System.Diagnostics.Debug.WriteLine("Reconnect");
            subscribeCall.Fire(SubscribeEvent.Reconnect);
        }

        private void SubscriptionChange()
        {
            System.Diagnostics.Debug.WriteLine("SubscriptionChange");
            subscribeCall.Fire(SubscribeEvent.SubscriptionChange);
        }

        public override string ToString()
        {
            return $"{nameof(SubscribeOperation<T>)}[state={subscribeCall.State}]";
        }
    }

    public class SubscribeStateMachine
    {
        StateMachine<SubscribeState, SubscribeEvent> subscribeCall = new StateMachine<SubscribeState, SubscribeEvent>(SubscribeState.Stopped);
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

        public SubscribeStateMachine()
        {

            //subscribeCall.Configure(SubscribeState)
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
