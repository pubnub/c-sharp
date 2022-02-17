using System;
using System.Collections;
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

        private StateMachine<SubscribeState, SubscribeEvent> subscribeCall = new StateMachine<SubscribeState, SubscribeEvent>(SubscribeState.Stopped);
        private StateMachine<SubscribeState, SubscribeEvent>.TriggerWithParameters<SubscribeData> handshakeEventTrigger;
        private StateMachine<SubscribeState, SubscribeEvent>.TriggerWithParameters<SubscribeData> receiveMessagesEventTrigger;
        //private StateMachine<SubscribeState, SubscribeEvent>.TriggerWithParameters<SubscribeData> deliveringMessagesEventTrigger;

        public StatelessSubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;

            handshakeEventTrigger = subscribeCall.SetTriggerParameters<SubscribeData>(SubscribeEvent.HandshakeSuccess);
            receiveMessagesEventTrigger = subscribeCall.SetTriggerParameters<SubscribeData>(SubscribeEvent.ReceiveSuccess);
            //deliveringMessagesEventTrigger = subscribeCall.SetTriggerParameters<SubscribeData>(SubscribeEvent.Done);

            subscribeCall.Configure(SubscribeState.Stopped)
                .Permit(SubscribeEvent.Subscribe, SubscribeState.Handshaking);

            subscribeCall.Configure(SubscribeState.Handshaking)
                .OnEntry(async () => 
                {
                    System.Diagnostics.Debug.WriteLine($"OnEntry: State={subscribeCall.State} ");
                    await OnEntryHandshaking(); 
                })
                .OnExit((data) =>
                {
                    System.Diagnostics.Debug.WriteLine($"OnExit: Trigger={data.Trigger}; Source={data.Source}; Destination={data.Destination}; IsRetrant={data.IsReentry.ToString()}");
                })
                .Permit(SubscribeEvent.HandshakeSuccess, SubscribeState.Receiving)
                .Permit(SubscribeEvent.Giveup, SubscribeState.HandshakeFailed)
                .PermitReentry(SubscribeEvent.SubscriptionChange)
                .PermitReentry(SubscribeEvent.Fail);


            subscribeCall.Configure(SubscribeState.HandshakeFailed)
                .OnEntry(async () => 
                {
                    System.Diagnostics.Debug.WriteLine($"OnEntry: State={subscribeCall.State} ");
                    await OnEntryHandshaking(); 
                })
                .OnExit((data) =>
                {
                    System.Diagnostics.Debug.WriteLine($"OnExit: Trigger={data.Trigger}; Source={data.Source}; Destination={data.Destination}; IsRetrant={data.IsReentry.ToString()}");
                })
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

            //var receiveSubscribeSuccessEventTrigger = subscribeCall.SetTriggerParameters<TimetokenAndRegion>(SubscribeEvent.ReceiveSuccess);
            subscribeCall.Configure(SubscribeState.Receiving)
                .OnEntryFrom(handshakeEventTrigger, async (data) =>
                {
                    System.Diagnostics.Debug.WriteLine($"OnEntry: State={subscribeCall.State} ");
                    await OnEntryReceiving(data);
                })
                .OnEntryFrom(receiveMessagesEventTrigger, async (data) =>
                {
                    System.Diagnostics.Debug.WriteLine($"OnEntry: State={subscribeCall.State} ");
                    await OnEntryReceiving(data);
                })
                .OnExit((data) =>
                {
                    System.Diagnostics.Debug.WriteLine($"OnExit: Trigger={data.Trigger}; Source={data.Source}; Destination={data.Destination}; IsRetrant={data.IsReentry.ToString()}");
                })
                //.Permit(SubscribeEvent.ReceiveSuccess, SubscribeState.Receiving)
                .Permit(SubscribeEvent.Fail, SubscribeState.Reconnecting)
                //.Permit(SubscribeEvent.ReceiveSuccess, SubscribeState.Receiving)
                .PermitReentry(SubscribeEvent.ReceiveSuccess)
                //.PermitReentryIf(SubscribeEvent.ReceiveSuccess, ()=> subscribeCall.IsInState(SubscribeState.Receiving))
                .PermitReentry(SubscribeEvent.SubscriptionChange);
                //.PermitReentryIf(SubscribeEvent.Done, () => subscribeCall.IsInState(SubscribeState.Receiving));
                //.Ignore(SubscribeEvent.Done);
                //.PermitReentry(SubscribeEvent.ReceiveSuccess);


            //subscribeCall.Configure(SubscribeState.Delivering)
            //    //.SubstateOf(SubscribeState.Receiving)
            //    //.OnEntryFrom(receiveMessagesEventTrigger, async (data) => await OnEntryDelivering(data))
            //    //.OnEntryFrom(deliveringMessagesEventTrigger, async (data) => await OnEntryDelivering(data))
            //    .OnEntryFrom(receiveMessagesEventTrigger, async (data) => 
            //    {
            //        System.Diagnostics.Debug.WriteLine($"OnEntry: State={subscribeCall.State} ");
            //        await OnEntryDelivering(data);
            //    })
            //    .OnExit((data) => 
            //    {
            //        System.Diagnostics.Debug.WriteLine($"OnExit: Trigger={data.Trigger}; Source={data.Source}; Destination={data.Destination}; IsRetrant={data.IsReentry.ToString()}");
            //        //subscribeCall.Fire(SubscribeEvent.Done);
            //        if (data != null && data.Parameters != null && data.Parameters.Count() > 0)
            //        {
            //            //subscribeCall.Fire(receiveMessagesEventTrigger, data.Parameters[0]);
                        
            //        }
            //    })
            //    .OnEntryFrom(deliveringMessagesEventTrigger, async(data) => 
            //    {
            //        System.Diagnostics.Debug.WriteLine($"OnEntry: State={subscribeCall.State} ");
            //        await OnEntryDelivering(data);
            //    })
            //    //.Ignore(SubscribeEvent.ReceiveSuccess)
            //    .Permit(SubscribeEvent.Done, SubscribeState.Receiving);
        }

        
        internal async Task<CancellationToken> OnEntryHandshaking()
        {
            System.Diagnostics.Debug.WriteLine("OnEntryHandshaking");
            CancellationTokenSource handShakeTokenSource = new CancellationTokenSource();
            Tuple<string, PNStatus>  handshakeResponse = await Handshake(handShakeTokenSource.Token);
            PNStatus status = handshakeResponse.Item2;
            if (!status.Error && status.StatusCode == 200)
            {
                System.Diagnostics.Debug.WriteLine($"Handshake success. {handshakeResponse.Item1}");

                List<object> handshakeResult = null;
                string jsonString = handshakeResponse.Item1;
                object deserializedResult = jsonLibrary.DeserializeToObject(jsonString);
                List<object> result1 = ((IEnumerable)deserializedResult).Cast<object>().ToList();

                if (result1 != null && result1.Count > 0)
                {
                    handshakeResult = result1;
                }

                long jsonTimetoken = GetTimetokenFromMultiplexResult(handshakeResult);
                int jsonRegion = GetRegionFromMultiplexResult(handshakeResult);

                SubscribeData handshakeData = new SubscribeData();
                handshakeData.Timetoken = jsonTimetoken;
                handshakeData.Region = jsonRegion;
                subscribeCall.Fire(handshakeEventTrigger, handshakeData);

                //subscribeCall.Fire(receiveMessagesEventTrigger, handshakeData);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Handshake FAILED. {jsonLibrary.SerializeToJsonString(status)}");
                subscribeCall.Fire(SubscribeEvent.Fail);
            }
            return handShakeTokenSource.Token;
        }

        internal async Task<CancellationToken> OnEntryReceiving(SubscribeData data)
        {
            System.Diagnostics.Debug.WriteLine("OnEntryReceiving");
            CancellationTokenSource receivingTokenSource = new CancellationTokenSource();
            Tuple<string, PNStatus> receiveMessagesResponse = await ReceiveMessages(data.Timetoken, data.Region, receivingTokenSource.Token);
            PNStatus status = receiveMessagesResponse.Item2;
            if (!status.Error && status.StatusCode == 200)
            {
                System.Diagnostics.Debug.WriteLine($"ReceiveMessages success. {receiveMessagesResponse.Item1}");

                List<object> receiveMessagesResult = null;
                string jsonString = receiveMessagesResponse.Item1;
                object deserializedResult = jsonLibrary.DeserializeToObject(jsonString);
                List<object> result1 = ((IEnumerable)deserializedResult).Cast<object>().ToList();

                if (result1 != null && result1.Count > 0)
                {
                    receiveMessagesResult = result1;
                }

                long jsonTimetoken = GetTimetokenFromMultiplexResult(receiveMessagesResult);
                int jsonRegion = GetRegionFromMultiplexResult(receiveMessagesResult);

                SubscribeData receiveMessagesData = new SubscribeData();
                receiveMessagesData.Timetoken = jsonTimetoken;
                receiveMessagesData.Region = jsonRegion;
                receiveMessagesData.Message = $"Hello...I got some message at tt={jsonTimetoken} with region={jsonRegion}";

                System.Diagnostics.Debug.WriteLine($"MESSAGE DELIVERED {receiveMessagesData.Message}");
                //Announce()

                subscribeCall.Fire(receiveMessagesEventTrigger, receiveMessagesData);

            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ReceiveMessages FAILED. {receiveMessagesResponse.Item1}");
            }
            return receivingTokenSource.Token;

        }

//        internal async Task<CancellationToken> OnEntryDelivering(SubscribeData obj)
//        {
//            System.Diagnostics.Debug.WriteLine("OnEntryDelivering");
//            CancellationTokenSource deliveringTokenSource = new CancellationTokenSource();

//#if NET40
//#else
//            await Task.Delay(1);
//#endif
//            System.Diagnostics.Debug.WriteLine($"MESSAGE DELIVERED: {obj.Message}");

//            subscribeCall.Fire(deliveringMessagesEventTrigger, obj);
//            //subscribeCall.Fire(receiveMessagesEventTrigger, obj);

//            //if (subscribeCall.CanFire(SubscribeEvent.Done))
//            //{
//            //    subscribeCall.Fire(SubscribeEvent.Done);
//            //}

//            return deliveringTokenSource.Token;
//        }

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

        private List<SubscribeMessage> GetMessageFromMultiplexResult(List<object> result)
        {
            List<object> jsonMessageList = null;
            List<SubscribeMessage> msgList = new List<SubscribeMessage>();

            Dictionary<string, object> messageDicObj = jsonLibrary.ConvertToDictionaryObject(result[1]);
            if (messageDicObj != null && messageDicObj.Count > 0 && messageDicObj.ContainsKey("m"))
            {
                jsonMessageList = messageDicObj["m"] as List<object>;
            }
            else
            {
                messageDicObj = jsonLibrary.ConvertToDictionaryObject(result[0]);
                if (messageDicObj != null && messageDicObj.Count > 0 && messageDicObj.ContainsKey("m"))
                {
                    jsonMessageList = messageDicObj["m"] as List<object>;
                }
            }

            if (jsonMessageList != null && jsonMessageList.Count > 0)
            {
                foreach (Dictionary<string, object> dicItem in jsonMessageList)
                {
                    if (dicItem.Count > 0)
                    {
                        SubscribeMessage msg = new SubscribeMessage();
                        foreach (string key in dicItem.Keys)
                        {
                            switch (key.ToLowerInvariant())
                            {
                                case "a":
                                    msg.Shard = dicItem[key].ToString();
                                    break;
                                case "b":
                                    msg.SubscriptionMatch = dicItem[key].ToString();
                                    break;
                                case "c":
                                    msg.Channel = dicItem[key].ToString();
                                    break;
                                case "d":
                                    msg.Payload = dicItem[key];
                                    break;
                                case "e":
                                    int subscriptionTypeIndicator;
                                    Int32.TryParse(dicItem[key].ToString(), out subscriptionTypeIndicator);
                                    msg.MessageType = subscriptionTypeIndicator;
                                    break;
                                case "f":
                                    msg.Flags = dicItem[key].ToString();
                                    break;
                                case "i":
                                    msg.IssuingClientId = dicItem[key].ToString();
                                    break;
                                case "k":
                                    msg.SubscribeKey = dicItem[key].ToString();
                                    break;
                                case "s":
                                    int seqNum;
                                    Int32.TryParse(dicItem[key].ToString(), out seqNum);
                                    msg.SequenceNumber = seqNum;
                                    break;
                                case "o":
                                    Dictionary<string, object> ttOriginMetaData = jsonLibrary.ConvertToDictionaryObject(dicItem[key]);
                                    if (ttOriginMetaData != null && ttOriginMetaData.Count > 0)
                                    {
                                        TimetokenMetadata ttMeta = new TimetokenMetadata();

                                        foreach (string metaKey in ttOriginMetaData.Keys)
                                        {
                                            if (metaKey.ToLowerInvariant().Equals("t", StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                long timetoken;
                                                Int64.TryParse(ttOriginMetaData[metaKey].ToString(), out timetoken);
                                                ttMeta.Timetoken = timetoken;
                                            }
                                            else if (metaKey.ToLowerInvariant().Equals("r", StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                ttMeta.Region = ttOriginMetaData[metaKey].ToString();
                                            }
                                        }
                                        msg.OriginatingTimetoken = ttMeta;
                                    }
                                    break;
                                case "p":
                                    Dictionary<string, object> ttPublishMetaData = jsonLibrary.ConvertToDictionaryObject(dicItem[key]);
                                    if (ttPublishMetaData != null && ttPublishMetaData.Count > 0)
                                    {
                                        TimetokenMetadata ttMeta = new TimetokenMetadata();

                                        foreach (string metaKey in ttPublishMetaData.Keys)
                                        {
                                            string currentMetaKey = metaKey.ToLowerInvariant();

                                            if (currentMetaKey.Equals("t", StringComparison.OrdinalIgnoreCase))
                                            {
                                                long timetoken;
                                                Int64.TryParse(ttPublishMetaData[metaKey].ToString(), out timetoken);
                                                ttMeta.Timetoken = timetoken;
                                            }
                                            else if (currentMetaKey.Equals("r", StringComparison.OrdinalIgnoreCase))
                                            {
                                                ttMeta.Region = ttPublishMetaData[metaKey].ToString();
                                            }
                                        }
                                        msg.PublishTimetokenMetadata = ttMeta;
                                    }
                                    break;
                                case "u":
                                    msg.UserMetadata = dicItem[key];
                                    break;
                                default:
                                    break;
                            }
                        }

                        msgList.Add(msg);
                    }
                }
            }
            return msgList;
        }
    }

    public class SubscribeData
    {
        public long Timetoken { get; set; }
        public int Region { get; set; }
        public object Message { get; set; }
    }
    public class SubscribeStateMachine
    {
        //StateMachine<SubscribeState, SubscribeEvent> subscribeCall = new StateMachine<SubscribeState, SubscribeEvent>(SubscribeState.Stopped);
        //public enum SubscribeState
        //{
        //    HandshakeFailed,
        //    Unsubscribed,
        //    ReconnectionFailed,
        //    Handshaking,
        //    Preparing,
        //    Stopped,
        //    Reconnecting,
        //    Receiving,
        //    Delivering
        //}

        //public enum SubscribeEvent
        //{
        //    Reconnect,
        //    SubscriptionChange,
        //    Fail,
        //    Giveup,
        //    Success,
        //    Subscribe,
        //    Restore,
        //    Disconnect,
        //    Done
        //}

        public SubscribeStateMachine()
        {

            //subscribeCall.Configure(SubscribeState)
        }

        //public SubscribeState ChangeSubscribeState(SubscribeState current, SubscribeEvent subscribeEvt)
        //{
        //    SubscribeState ret;
        //    switch (current)
        //    {
        //        case SubscribeState.Handshaking:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.SubscriptionChange:
        //                case SubscribeEvent.Fail:
        //                    ret = SubscribeState.Handshaking;
        //                    break;
        //                case SubscribeEvent.Success:
        //                    ret = SubscribeState.Receiving;
        //                    break;
        //                case SubscribeEvent.Giveup:
        //                    ret = SubscribeState.HandshakeFailed;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.HandshakeFailed:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.SubscriptionChange:
        //                case SubscribeEvent.Reconnect:
        //                    ret = SubscribeState.Handshaking;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.Reconnecting:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.SubscriptionChange:
        //                case SubscribeEvent.Fail:
        //                    ret = SubscribeState.Reconnecting;
        //                    break;
        //                case SubscribeEvent.Success:
        //                    ret = SubscribeState.Delivering;
        //                    break;
        //                case SubscribeEvent.Giveup:
        //                    ret = SubscribeState.ReconnectionFailed;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.ReconnectionFailed:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.SubscriptionChange:
        //                case SubscribeEvent.Reconnect:
        //                    ret = SubscribeState.Reconnecting;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.Unsubscribed:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.Subscribe:
        //                    ret = SubscribeState.Handshaking;
        //                    break;
        //                case SubscribeEvent.Restore:
        //                    ret = SubscribeState.Reconnecting;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.Preparing:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.Reconnect:
        //                    ret = SubscribeState.Handshaking;
        //                    break;
        //                case SubscribeEvent.SubscriptionChange:
        //                    ret = SubscribeState.Preparing;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.Stopped:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.Reconnect:
        //                    ret = SubscribeState.Reconnecting;
        //                    break;
        //                case SubscribeEvent.SubscriptionChange:
        //                    ret = SubscribeState.Stopped;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.Receiving:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.Success:
        //                    ret = SubscribeState.Delivering;
        //                    break;
        //                case SubscribeEvent.Fail:
        //                    ret = SubscribeState.Reconnecting;
        //                    break;
        //                case SubscribeEvent.SubscriptionChange:
        //                    ret = SubscribeState.Receiving;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        case SubscribeState.Delivering:
        //            switch (subscribeEvt)
        //            {
        //                case SubscribeEvent.Done:
        //                    ret = SubscribeState.Receiving;
        //                    break;
        //                default:
        //                    throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //            }
        //            break;
        //        default:
        //            throw new Exception($"State: {current}; Event:{subscribeEvt} has no transition/effect");
        //    }

        //    return ret;

        //}

    }
}
