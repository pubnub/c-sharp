using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class StatelessSubscribeOperation<T> //: Stateless_PubnubCoreBase
    {
        private SubscribeState<T> state = new HandshakingState<T>(null, SubscribeEvent.SubscriptionChange);

        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        public List<string> ChannelList = null;
        public List<string> ChannelgroupList = null;
        private Dictionary<string, object> queryParam;

        public PNConfiguration Config { get { return config; } }
        public IJsonPluggableLibrary JsonLibrary { get { return jsonLibrary; } }
        public IPubnubUnitTest Unit { get { return unit; } }
        public IPubnubLog PubnubLog { get { return pubnubLog; } }
        public TelemetryManager PubnubTelemetryMgr { get { return pubnubTelemetryMgr; } }
        public TokenManager PubnubTokenMgr { get { return pubnubTokenMgr; } }

        public StatelessSubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;
            //state.Config = pubnubConfig;
        }

        public SubscribeState<T> State
        {
            get { return state; }
            set { state = value; }
        }

        //internal async Task<CancellationToken> OnEntryReceiving(SubscribeData data)
        //{
        //    System.Diagnostics.Debug.WriteLine("OnEntryReceiving");
        //    CancellationTokenSource receivingTokenSource = new CancellationTokenSource();
        //    Tuple<string, PNStatus> receiveMessagesResponse = await ReceiveMessages(data.Timetoken, data.Region, receivingTokenSource.Token);
        //    PNStatus status = receiveMessagesResponse.Item2;
        //    if (!status.Error && status.StatusCode == 200)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"ReceiveMessages success. {receiveMessagesResponse.Item1}");

        //        List<object> receiveMessagesResult = null;
        //        string jsonString = receiveMessagesResponse.Item1;
        //        object deserializedResult = jsonLibrary.DeserializeToObject(jsonString);
        //        List<object> result1 = ((IEnumerable)deserializedResult).Cast<object>().ToList();

        //        if (result1 != null && result1.Count > 0)
        //        {
        //            receiveMessagesResult = result1;
        //        }

        //        long jsonTimetoken = GetTimetokenFromMultiplexResult(receiveMessagesResult);
        //        int jsonRegion = GetRegionFromMultiplexResult(receiveMessagesResult);

        //        SubscribeData receiveMessagesData = new SubscribeData();
        //        receiveMessagesData.Timetoken = jsonTimetoken;
        //        receiveMessagesData.Region = jsonRegion;
        //        receiveMessagesData.Message = $"Hello...I got some message at tt={jsonTimetoken} with region={jsonRegion}";

        //        System.Diagnostics.Debug.WriteLine($"MESSAGE DELIVERED {receiveMessagesData.Message}");
        //        //Announce()

        //        subscribeCall.Fire(receiveMessagesEventTrigger, receiveMessagesData);

        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine($"ReceiveMessages FAILED. {receiveMessagesResponse.Item1}");
        //    }
        //    return receivingTokenSource.Token;

        //}

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
            ChannelList = channels;
            return this;
        }

        public StatelessSubscribeOperation<T> ChannelGroups(List<string> channelGroups)
        {
            ChannelgroupList = channelGroups;
            return this;
        }

        public StatelessSubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }


        public async Task<Tuple<string, PNStatus>> ReceiveMessages(long timetoken, int region, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("ReceiveMessages");

            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            Tuple<string, PNStatus> receiveMessageResponse = await subscribeManager.ReceiveMessages<T>(PNOperationType.PNSubscribeOperation, ChannelList.ToArray(), (ChannelgroupList != null) ? ChannelgroupList.ToArray() : null, timetoken, region, false, null, queryParam, cancellationToken);

            System.Diagnostics.Debug.WriteLine(receiveMessageResponse.Item1);
            System.Diagnostics.Debug.WriteLine(jsonLibrary.SerializeToJsonString(receiveMessageResponse.Item2));

            return receiveMessageResponse;
        }

        public async Task<CancellationToken> IamHere()
        {
            CancellationTokenSource iamHereTokenSource = new CancellationTokenSource();
            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            await subscribeManager.IamHere(PNOperationType.PNUnsubscribeOperation, ChannelList.ToArray(), ChannelgroupList.ToArray(), iamHereTokenSource.Token);

            return iamHereTokenSource.Token;
        }

        public async Task<CancellationToken> IamAway(string unsubChannel, string unsubChannelGroup)
        {
            CancellationTokenSource iamAwayTokenSource = new CancellationTokenSource();

            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            await subscribeManager.IAmAway<T>(PNOperationType.PNUnsubscribeOperation, unsubChannel, unsubChannelGroup, null, iamAwayTokenSource.Token);

            return iamAwayTokenSource.Token;
        }

        public async Task Execute()
        {
            await SubscriptionChange();
        }

        private void Reconnect()
        {
            System.Diagnostics.Debug.WriteLine("Reconnect");
            //subscribeCall.Fire(SubscribeEvent.Reconnect);
        }

        private async Task SubscriptionChange()
        {
            System.Diagnostics.Debug.WriteLine("SubscriptionChange");
            state = new HandshakingState<T>(this, SubscribeEvent.SubscriptionChange);
            SubscribeState<T> nextState = state.ChangeSubscribeState(this, SubscribeEvent.SubscriptionChange);
            await nextState.Request();
        }

        public override string ToString()
        {
            return $"{nameof(SubscribeOperation<T>)}[state={state.GetType().Name}]";
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
    public abstract class SubscribeState<T>
    {
        protected StatelessSubscribeOperation<T> subscribeOperation;
        internal PNConfiguration config;
        internal IJsonPluggableLibrary jsonLibrary;
        internal IPubnubUnitTest unit;
        internal IPubnubLog pubnubLog;
        internal TelemetryManager pubnubTelemetryMgr;
        internal TokenManager pubnubTokenMgr;

        protected long timetoken;
        public StatelessSubscribeOperation<T> SubscribeOperation
        {
            get { return subscribeOperation; }
            set { subscribeOperation = value; }
        }

        public PNConfiguration Config
        {
            get { return config; }
            set { config = value; }
        }

        public List<string> Channels { get; set; }
        public List<string> ChannelGroups { get; set; }

        public Dictionary<string, object> QueryParam { get; set; }

        public abstract string Name { get; }
        public abstract Task<CancellationToken> Request();
        public SubscribeData SubscribeData { get; set; }


        public SubscribeState<T> ChangeSubscribeState(StatelessSubscribeOperation<T> currentOperation, SubscribeEvent subscribeEvt)
        {
            SubscribeState<T> ret;

            subscribeOperation = currentOperation;
            Channels = currentOperation.ChannelList;
            ChannelGroups = currentOperation.ChannelgroupList;
            SubscribeState<T> currentState = currentOperation.State;
            switch (currentState.Name)
            {
                case "HandshakingState":
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.SubscriptionChange:
                        case SubscribeEvent.Fail:
                            ret = new HandshakingState<T>(subscribeOperation, subscribeEvt);
                            break;
                        case SubscribeEvent.Success:
                            ret = new ReceivingState<T>(subscribeOperation, subscribeEvt);
                            break;
                        case SubscribeEvent.Giveup:
                            ret = new HandshakeFailedState<T>(subscribeOperation, subscribeEvt);
                            break;
                        default:
                            throw new Exception($"State: {currentState}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case "HandshakeFailedState":
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.SubscriptionChange:
                        case SubscribeEvent.Reconnect:
                            ret = new HandshakingState<T>(subscribeOperation, subscribeEvt);
                            break;
                        default:
                            throw new Exception($"State: {currentState}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                case "ReceivingState":
                    switch (subscribeEvt)
                    {
                        case SubscribeEvent.Success:
                            ret = new ReceivingState<T>(subscribeOperation, subscribeEvt);
                            break;
                        case SubscribeEvent.Fail:
                            ret = new ReconnectingState<T>(subscribeOperation, subscribeEvt);
                            break;
                        case SubscribeEvent.SubscriptionChange:
                            ret = new ReceivingState<T>(subscribeOperation, subscribeEvt);
                            break;
                        default:
                            throw new Exception($"State: {currentState}; Event:{subscribeEvt} has no transition/effect");
                    }
                    break;
                default:
                    throw new Exception($"State: {currentState}; Event:{subscribeEvt} has no transition/effect");
            }

            return ret;

        }

        protected long GetTimetokenFromMultiplexResult(List<object> result)
        {
            long jsonTimetoken = 0;
            Dictionary<string, object> timetokenObj = subscribeOperation.JsonLibrary.ConvertToDictionaryObject(result[0]);

            if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
            {
                Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("t"))
                {
                    Int64.TryParse(timeAndRegionDictionary["t"].ToString(), out jsonTimetoken);
                }
            }
            else
            {
                timetokenObj = jsonLibrary.ConvertToDictionaryObject(result[1]);
                if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
                {
                    Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                    if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("t"))
                    {
                        Int64.TryParse(timeAndRegionDictionary["t"].ToString(), out jsonTimetoken);
                    }
                }
                else
                {
                    Int64.TryParse(result[1].ToString(), out jsonTimetoken);
                }
            }

            return jsonTimetoken;
        }

        protected int GetRegionFromMultiplexResult(List<object> result)
        {
            int jsonRegion = 0;
            Dictionary<string, object> timetokenObj = subscribeOperation.JsonLibrary.ConvertToDictionaryObject(result[0]);

            if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
            {
                Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("r"))
                {
                    Int32.TryParse(timeAndRegionDictionary["r"].ToString(), out jsonRegion);
                }
            }
            else
            {
                timetokenObj = jsonLibrary.ConvertToDictionaryObject(result[1]);
                if (timetokenObj != null && timetokenObj.Count > 0 && timetokenObj.ContainsKey("t"))
                {
                    Dictionary<string, object> timeAndRegionDictionary = timetokenObj["t"] as Dictionary<string, object>;
                    if (timeAndRegionDictionary != null && timeAndRegionDictionary.Count > 0 && timeAndRegionDictionary.ContainsKey("r"))
                    {
                        Int32.TryParse(timeAndRegionDictionary["r"].ToString(), out jsonRegion);
                    }
                }
            }

            return jsonRegion;
        }

        private List<SubscribeMessage> GetMessageFromMultiplexResult(List<object> result)
        {
            List<object> jsonMessageList = null;
            List<SubscribeMessage> msgList = new List<SubscribeMessage>();

            Dictionary<string, object> messageDicObj = subscribeOperation.JsonLibrary.ConvertToDictionaryObject(result[1]);
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
                                    Dictionary<string, object> ttOriginMetaData = subscribeOperation.JsonLibrary.ConvertToDictionaryObject(dicItem[key]);
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
                                    Dictionary<string, object> ttPublishMetaData = subscribeOperation.JsonLibrary.ConvertToDictionaryObject(dicItem[key]);
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

    public class HandshakingState<T> : SubscribeState<T>
    {
        SubscribeEvent currentTrigger;
        public readonly string subscribeStateName;

        public override string Name 
        { 
            get { return subscribeStateName; }
        }

        public HandshakingState(StatelessSubscribeOperation<T> subscribeOperation, SubscribeEvent subscribeEvent)
        {
            subscribeStateName = "HandshakingState";
            SubscribeOperation = subscribeOperation;
            if (subscribeOperation != null)
            {
                Channels = subscribeOperation.ChannelList;
                ChannelGroups = subscribeOperation.ChannelgroupList;
            }
            currentTrigger = subscribeEvent;
        }

        private void Initialize()
        {
            timetoken = 0;
        }

        public async override Task<CancellationToken> Request()
        {
            CancellationTokenSource handShakeTokenSource = new CancellationTokenSource();
            switch (currentTrigger)
            {
                case SubscribeEvent.SubscriptionChange:
                case SubscribeEvent.Fail:
                    await InvokeHandshakingStateEffects(handShakeTokenSource.Token);
                    break;
                case SubscribeEvent.Giveup:
                    break;
                case SubscribeEvent.Success:
                    break;
                default:
                    throw new Exception($"Invalid event {currentTrigger} for {this.GetType().Name}");
            }
            return handShakeTokenSource.Token;
        }

        private async Task<bool> InvokeHandshakingStateEffects(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("HandshakingStateOnSubscriptionChangeOrFail");
            bool ret = false;
            Tuple<string, PNStatus> handshakeResponse = await Handshake(cancellationToken);
            PNStatus status = handshakeResponse.Item2;
            if (!status.Error && status.StatusCode == 200)
            {
                System.Diagnostics.Debug.WriteLine($"Handshake success. {handshakeResponse.Item1}");

                List<object> handshakeResult = null;
                string jsonString = handshakeResponse.Item1;
                object deserializedResult = subscribeOperation.JsonLibrary.DeserializeToObject(jsonString);
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

                ret = true;
                
                SubscribeState<T> nextState = ChangeSubscribeState(SubscribeOperation, SubscribeEvent.Success);
                nextState.SubscribeData = handshakeData;
                await nextState.Request();

            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Handshake FAILED. {jsonLibrary.SerializeToJsonString(status)}");
                
                SubscribeState<T> nextState = ChangeSubscribeState(SubscribeOperation, SubscribeEvent.Fail);
                await nextState.Request();
            }
            return ret;
        }

        private async Task<Tuple<string, PNStatus>> Handshake(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("Handshake");

            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(subscribeOperation.Config, subscribeOperation.JsonLibrary, subscribeOperation.Unit, subscribeOperation.PubnubLog, subscribeOperation.PubnubTelemetryMgr, subscribeOperation.PubnubTokenMgr);
            Tuple<string, PNStatus> handshakeResponse = await subscribeManager.Handshake<string>(PNOperationType.PNSubscribeOperation, (Channels != null) ? Channels.ToArray() : null, (ChannelGroups != null) ? ChannelGroups.ToArray() : null, null, QueryParam, cancellationToken);

            System.Diagnostics.Debug.WriteLine(handshakeResponse.Item1);
            System.Diagnostics.Debug.WriteLine(subscribeOperation.JsonLibrary.SerializeToJsonString(handshakeResponse.Item2));

            return handshakeResponse;
        }

    }

    public class HandshakeFailedState<T> : SubscribeState<T>
    {
        SubscribeEvent currentTrigger;
        public readonly string subscribeStateName;

        public override string Name
        {
            get { return subscribeStateName; }
        }
        public HandshakeFailedState(StatelessSubscribeOperation<T> subscribeOperation, SubscribeEvent subscribeEvent)
        {
            subscribeStateName = "HandshakeFailedState";
            base.SubscribeOperation = subscribeOperation;
            config = subscribeOperation.State.Config;
            currentTrigger = subscribeEvent;
        }
        public override Task<CancellationToken> Request()
        {
            throw new NotImplementedException();
        }
    }

    public class ReceivingState<T> : SubscribeState<T>
    {
        SubscribeEvent currentTrigger;
        public readonly string subscribeStateName;

        public override string Name
        {
            get { return subscribeStateName; }
        }

        public ReceivingState(StatelessSubscribeOperation<T> subscribeOperation, SubscribeEvent subscribeEvent)
        {
            subscribeStateName = "ReceivingState";
            SubscribeOperation = subscribeOperation;
            if (subscribeOperation != null)
            {
                Channels = subscribeOperation.ChannelList;
                ChannelGroups = subscribeOperation.ChannelgroupList;
            }
            currentTrigger = subscribeEvent;
        }
        public async override Task<CancellationToken> Request()
        {
            CancellationTokenSource receiveTokenSource = new CancellationTokenSource();
            switch (currentTrigger)
            {
                case SubscribeEvent.SubscriptionChange:
                case SubscribeEvent.Success:
                    await InvokeReceivingStateEffects(receiveTokenSource.Token);
                    break;
                case SubscribeEvent.Giveup:
                    break;
                default:
                    throw new Exception($"Invalid event {currentTrigger} for {this.Name}");
            }
            return receiveTokenSource.Token;
        }

        private async Task<bool> InvokeReceivingStateEffects(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("InvokeReceivingStateEffects");
            bool ret = false;
            Tuple<string, PNStatus> receiveMessagesResponse = await ReceiveMessages(SubscribeData.Timetoken, SubscribeData.Region, cancellationToken);
            PNStatus status = receiveMessagesResponse.Item2;
            if (!status.Error && status.StatusCode == 200)
            {
                System.Diagnostics.Debug.WriteLine($"ReceiveMessages success. {receiveMessagesResponse.Item1}");

                List<object> handshakeResult = null;
                string jsonString = receiveMessagesResponse.Item1;
                object deserializedResult = subscribeOperation.JsonLibrary.DeserializeToObject(jsonString);
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

                ret = true;

                SubscribeState<T> nextState = ChangeSubscribeState(SubscribeOperation, SubscribeEvent.Success);
                nextState.SubscribeData = handshakeData;
                await nextState.Request();

            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Handshake FAILED. {jsonLibrary.SerializeToJsonString(status)}");

                SubscribeState<T> nextState = ChangeSubscribeState(SubscribeOperation, SubscribeEvent.Fail);
                await nextState.Request();
            }
            return ret;
        }

        private async Task<Tuple<string, PNStatus>> ReceiveMessages(long timetoken, int region, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("ReceiveMessages");

            StatelessSubscribeManager subscribeManager = new StatelessSubscribeManager(SubscribeOperation.Config, SubscribeOperation.JsonLibrary, SubscribeOperation.Unit, SubscribeOperation.PubnubLog, SubscribeOperation.PubnubTelemetryMgr, SubscribeOperation.PubnubTokenMgr);
            Tuple<string, PNStatus> receiveMessageResponse = await subscribeManager.ReceiveMessages<T>(PNOperationType.PNSubscribeOperation, (Channels != null) ? Channels.ToArray() : null, (ChannelGroups != null) ? ChannelGroups.ToArray() : null, timetoken, region, false, null, QueryParam, cancellationToken);

            System.Diagnostics.Debug.WriteLine(receiveMessageResponse.Item1);
            System.Diagnostics.Debug.WriteLine(subscribeOperation.JsonLibrary.SerializeToJsonString(receiveMessageResponse.Item2));

            return receiveMessageResponse;
        }

    }

    public class ReconnectingState<T> : SubscribeState<T>
    {
        SubscribeEvent currentTrigger;
        public readonly string subscribeStateName;

        public override string Name
        {
            get { return subscribeStateName; }
        }
        public ReconnectingState(StatelessSubscribeOperation<T> subscribeOperation, SubscribeEvent subscribeEvent)
        {
            subscribeStateName = "ReconnectingState";
            base.SubscribeOperation = subscribeOperation;
            config = subscribeOperation.State.Config;
            currentTrigger = subscribeEvent;
        }
        public override Task<CancellationToken> Request()
        {
            throw new NotImplementedException();
        }
    }

    public class UnsubscribedState<T> : SubscribeState<T>
    {
        SubscribeEvent currentTrigger;
        public readonly string subscribeStateName;

        public override string Name
        {
            get { return subscribeStateName; }
        }
        public UnsubscribedState(StatelessSubscribeOperation<T> subscribeOperation, SubscribeEvent subscribeEvent)
        {
            subscribeStateName = "UnsubscribedState";
            base.SubscribeOperation = subscribeOperation;
            config = subscribeOperation.State.Config;
            currentTrigger = subscribeEvent;
        }
        public override Task<CancellationToken> Request()
        {
            throw new NotImplementedException();
        }
    }
}
