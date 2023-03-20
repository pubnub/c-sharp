using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using PubnubApi.PubnubEventEngine;

namespace PubnubApi.EndPoint
{
    public class SubscribeOperation2<T> : PubnubCoreBase2
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private List<string> subscribeChannelNames = new List<string>();
        private List<string> subscribeChannelGroupNames = new List<string>();
        private long subscribeTimetoken = -1;
        private bool presenceSubscribeEnabled;
        private SubscribeManager2 manager;
        private Dictionary<string, object> queryParam;
        private EventEngine pnEventEngine;

        public SubscribeOperation2(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;

			var effectDispatcher = new EffectDispatcher();
			var eventEmitter = new EventEmitter();
			var handshakeEffect = new HandshakeEffectHandler(eventEmitter);
            handshakeEffect.LogCallback = LogCallback;
            handshakeEffect.HandshakeRequested += HandshakeEffect_HandshakeRequested;
			
            var receivingEffect = new ReceivingEffectHandler<string>(eventEmitter);
            receivingEffect.LogCallback = LogCallback;
            receivingEffect.ReceiveRequested += ReceivingEffect_ReceiveRequested;

			var reconnectionEffect = new ReconnectingEffectHandler<string>(eventEmitter);
			
            effectDispatcher.Register(EffectType.SendHandshakeRequest, handshakeEffect);
			effectDispatcher.Register(EffectType.ReceiveEventRequest, receivingEffect);
			effectDispatcher.Register(EffectType.ReconnectionAttempt, reconnectionEffect);

			pnEventEngine = new EventEngine(effectDispatcher, eventEmitter);

			var initState = pnEventEngine.CreateState(StateType.Unsubscribed)
				.OnEntry(() => { System.Diagnostics.Debug.WriteLine("Unsubscribed: OnEntry()"); return true; })
				.OnExit(() => { System.Diagnostics.Debug.WriteLine("Unsubscribed: OnExit()"); return true; })
				.On(EventType.SubscriptionChange, StateType.Handshaking);

			pnEventEngine.CreateState(StateType.Handshaking)
				.OnEntry(() => { System.Diagnostics.Debug.WriteLine("Handshaking: OnEntry()"); return true; })
				.OnExit(() => { System.Diagnostics.Debug.WriteLine("Handshaking: OnExit()"); return true; })
				.On(EventType.SubscriptionChange, StateType.Handshaking)
				.On(EventType.HandshakeSuccess, StateType.Receiving)
				.On(EventType.HandshakeFailed, StateType.Reconnecting)
				.Effect(EffectType.SendHandshakeRequest);

			pnEventEngine.CreateState(StateType.Receiving)
				.OnEntry(() => { System.Diagnostics.Debug.WriteLine("Receiving: OnEntry()"); return true; })
				.OnExit(() => { System.Diagnostics.Debug.WriteLine("Receiving: OnExit()"); return true; })
				.On(EventType.SubscriptionChange, StateType.Handshaking)
				.On(EventType.ReceiveSuccess, StateType.Receiving)
				.On(EventType.ReceiveFailed, StateType.Reconnecting)
				.Effect(EffectType.ReceiveEventRequest);

			pnEventEngine.CreateState(StateType.HandshakingFailed)
				.OnEntry(() => { System.Diagnostics.Debug.WriteLine("HandshakingFailed: OnEntry()"); return true; })
				.OnExit(() => { System.Diagnostics.Debug.WriteLine("HandshakingFailed: OnExit()"); return true; })
				.On(EventType.SubscriptionChange, StateType.Handshaking)
				.On(EventType.HandshakeSuccess, StateType.Receiving)
				.On(EventType.HandshakeFailed, StateType.Reconnecting);

			pnEventEngine.CreateState(StateType.Reconnecting)
				.OnEntry(() => { System.Diagnostics.Debug.WriteLine("Reconnecting: OnEntry()"); return true; })
				.OnExit(() => { System.Diagnostics.Debug.WriteLine("Reconnecting: OnExit()"); return true; })
				.On(EventType.SubscriptionChange, StateType.Handshaking)
				.On(EventType.HandshakeSuccess, StateType.Receiving)
				.On(EventType.HandshakeFailed, StateType.Reconnecting);

			pnEventEngine.InitialState(initState);

        }

        private void ReceivingEffect_ReceiveRequested(object sender, ReceiveRequestEventArgs e)
        {
            manager = new SubscribeManager2(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
            manager.CurrentPubnubInstance(PubnubInstance);
            
            string jsonResp = manager.HandshakeRequest<T>(PNOperationType.PNSubscribeOperation, e.ExtendedState.Channels.ToArray(), e.ExtendedState.ChannelGroups.ToArray(), e.ExtendedState.Timetoken, e.ExtendedState.Region, null, null).Result;

            e.ReceiveResponseCallback?.Invoke(jsonResp);
        }

        private void HandshakeEffect_HandshakeRequested(object sender, HandshakeRequestEventArgs e)
        {
            manager = new SubscribeManager2(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
            manager.CurrentPubnubInstance(PubnubInstance);
            
            string jsonResp = manager.HandshakeRequest<T>(PNOperationType.PNSubscribeOperation, e.ExtendedState.Channels.ToArray(), e.ExtendedState.ChannelGroups.ToArray(), e.ExtendedState.Timetoken, e.ExtendedState.Region, null, null).Result;

            e.HandshakeResponseCallback?.Invoke(jsonResp);
        }

        private void LogCallback(string log)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), log), config.LogVerbosity);
        }

        public SubscribeOperation2<T> Channels(string[] channels)
        {
            if (channels != null && channels.Length > 0 && !string.IsNullOrEmpty(channels[0]))
            {
                this.subscribeChannelNames.AddRange(channels);
            }
            return this;
        }

        public SubscribeOperation2<T> ChannelGroups(string[] channelGroups)
        {
            if (channelGroups != null && channelGroups.Length > 0 && !string.IsNullOrEmpty(channelGroups[0]))
            {
                this.subscribeChannelGroupNames.AddRange(channelGroups);
            }
            return this;
        }

        public SubscribeOperation2<T> WithTimetoken(long timetoken)
        {
            this.subscribeTimetoken = timetoken;
            return this;
        }

        public SubscribeOperation2<T> WithPresence()
        {
            this.presenceSubscribeEnabled = true;
            return this;
        }

        public SubscribeOperation2<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute()
        {
            if (this.subscribeChannelNames == null)
            {
                this.subscribeChannelNames = new List<string>();
            }

            if (this.subscribeChannelGroupNames == null)
            {
                this.subscribeChannelGroupNames = new List<string>();
            }

            if (this.presenceSubscribeEnabled)
            {
                List<string> presenceChannelNames = (this.subscribeChannelNames != null && this.subscribeChannelNames.Count > 0 && !string.IsNullOrEmpty(this.subscribeChannelNames[0])) 
                                                ? this.subscribeChannelNames.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", c)).ToList() : new List<string>();
                List<string> presenceChannelGroupNames = (this.subscribeChannelGroupNames != null && this.subscribeChannelGroupNames.Count > 0 && !string.IsNullOrEmpty(this.subscribeChannelGroupNames[0])) 
                                                ? this.subscribeChannelGroupNames.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", c)).ToList() : new List<string>();

                if (this.subscribeChannelNames != null && presenceChannelNames.Count > 0)
                {
                    this.subscribeChannelNames.AddRange(presenceChannelNames);
                }

                if (this.subscribeChannelGroupNames != null && presenceChannelGroupNames.Count > 0)
                {
                    this.subscribeChannelGroupNames.AddRange(presenceChannelGroupNames);
                }
            }

            string[] channelNames = this.subscribeChannelNames != null ? this.subscribeChannelNames.ToArray() : null;
            string[] channelGroupNames = this.subscribeChannelGroupNames != null ? this.subscribeChannelGroupNames.ToArray() : null;

            Subscribe(channelNames, channelGroupNames, this.queryParam);
        }

        private void Subscribe(string[] channels, string[] channelGroups, Dictionary<string, object> externalQueryParam)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length  == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            string channel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            PNPlatform.Print(config, pubnubLog);

            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, requested subscribe for channel(s)={1} and channel group(s)={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel, channelGroup), config.LogVerbosity);

            Dictionary<string, string> initialSubscribeUrlParams = new Dictionary<string, string>();
            if (this.subscribeTimetoken >= 0)
            {
                initialSubscribeUrlParams.Add("tt", this.subscribeTimetoken.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrEmpty(config.FilterExpression) && config.FilterExpression.Trim().Length > 0)
            {
                initialSubscribeUrlParams.Add("filter-expr", UriUtil.EncodeUriComponent(config.FilterExpression, PNOperationType.PNSubscribeOperation, false, false, false));
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                pnEventEngine.Subscribe(channels.ToList<string>(), channelGroups.ToList<string>());
                //manager = new SubscribeManager2(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
                //manager.CurrentPubnubInstance(PubnubInstance);
                //manager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channels, channelGroups, initialSubscribeUrlParams, externalQueryParam);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                pnEventEngine.Subscribe(channels.ToList<string>(), channelGroups.ToList<string>());
                //manager = new SubscribeManager2(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
                //manager.CurrentPubnubInstance(PubnubInstance);
                //manager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channels, channelGroups, initialSubscribeUrlParams, externalQueryParam);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal bool Retry(bool reconnect)
        {
            if (manager == null)
            {
                return false;
            }

            if (reconnect)
            {
                return manager.Reconnect<T>(false);
            }
            else
            {
                return manager.Disconnect();
            }
        }

        internal bool Retry(bool reconnect, bool resetSubscribeTimetoken)
        {
            if (manager == null)
            {
                return false;
            }

            if (reconnect)
            {
                return manager.Reconnect<T>(resetSubscribeTimetoken);
            }
            else
            {
                return manager.Disconnect();
            }
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
            if (!MultiChannelSubscribe.ContainsKey(instance.InstanceId))
            {
                MultiChannelSubscribe.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, long>());
            }
            if (!MultiChannelGroupSubscribe.ContainsKey(instance.InstanceId))
            {
                MultiChannelGroupSubscribe.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, long>());
            }
            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelLocalUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupLocalUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelUserState.ContainsKey(instance.InstanceId))
            {
                ChannelUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelReconnectTimer.ContainsKey(instance.InstanceId))
            {
                ChannelReconnectTimer.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Timer>());
            }
            if (!ChannelGroupReconnectTimer.ContainsKey(instance.InstanceId))
            {
                ChannelGroupReconnectTimer.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Timer>());
            }
            if (!SubscribeDisconnected.ContainsKey(instance.InstanceId))
            {
                SubscribeDisconnected.GetOrAdd(instance.InstanceId, false);
            }
            if (!LastSubscribeTimetoken.ContainsKey(instance.InstanceId))
            {
                LastSubscribeTimetoken.GetOrAdd(instance.InstanceId, 0);
            }
            if (!LastSubscribeRegion.ContainsKey(instance.InstanceId))
            {
                LastSubscribeRegion.GetOrAdd(instance.InstanceId, 0);
            }
            if (!SubscribeRequestTracker.ContainsKey(instance.InstanceId))
            {
                SubscribeRequestTracker.GetOrAdd(instance.InstanceId, DateTime.Now);
            }
        }
    }
}
