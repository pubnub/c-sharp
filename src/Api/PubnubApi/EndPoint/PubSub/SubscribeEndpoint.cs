using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using PubnubApi.EventEngine.Subscribe;
using PubnubApi;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EndPoint
{
    public class SubscribeEndpoint<T>: ISubscribeOperation<T>
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
        private Pubnub PubnubInstance;
        private SubscribeEventEngine subscribeEventEngine;
        private SubscribeEventEngineFactory subscribeEventEngineFactory { get; set; }
        private string instanceId { get; set; }
		public List<SubscribeCallback> SubscribeListenerList
        {
            get;
            set;
        } = new List<SubscribeCallback>();

        public SubscribeEndpoint(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager,SubscribeEventEngineFactory subscribeEventEngineFactory, string instanceId, Pubnub instance) 
        {
            PubnubInstance = instance;
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;
            this.subscribeEventEngineFactory = subscribeEventEngineFactory;
            this.instanceId = instanceId;
            if (unit != null) { unit.EventTypeList = new List<KeyValuePair<string, string>>(); }
        }

        public ISubscribeOperation<T> Channels(string[] channels)
        {
            if (channels != null && channels.Length > 0 && !string.IsNullOrEmpty(channels[0]))
            {
                this.subscribeChannelNames.AddRange(channels);
            }
            return this;
        }

        public ISubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            if (channelGroups != null && channelGroups.Length > 0 && !string.IsNullOrEmpty(channelGroups[0]))
            {
                this.subscribeChannelGroupNames.AddRange(channelGroups);
            }
            return this;
        }

        public ISubscribeOperation<T> WithTimetoken(long timetoken)
        {
            this.subscribeTimetoken = timetoken;
            return this;
        }

        public ISubscribeOperation<T> WithPresence()
        {
            this.presenceSubscribeEnabled = true;
            return this;
        }

        public ISubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
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
            SubscriptionCursor cursor = null;
            if (subscribeTimetoken >= 1)
            {
                cursor = new SubscriptionCursor { Timetoken = subscribeTimetoken, Region = 0 };
            }
            Subscribe(channelNames, channelGroupNames, cursor, this.queryParam);
        }

        private void Subscribe(string[] channels, string[] channelGroups, SubscriptionCursor cursor, Dictionary<string, object> externalQueryParam)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0))
            {
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}

			if (this.subscribeEventEngineFactory.HasEventEngine(instanceId))
            {
                subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(instanceId);
			}
            else
            {
				var subscribeManager = new SubscribeManager2(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
				subscribeEventEngine = subscribeEventEngineFactory.InitializeEventEngine(instanceId, PubnubInstance, config, subscribeManager, StatusEmitter, MessageEmitter);
                subscribeEventEngine.OnStateTransition += SubscribeEventEngine_OnStateTransition;
                subscribeEventEngine.OnEventQueued += SubscribeEventEngine_OnEventQueued;
                subscribeEventEngine.OnEffectDispatch += SubscribeEventEngine_OnEffectDispatch;
			}
			subscribeEventEngine.Subscribe<T>(channels, channelGroups, cursor);
		}

        private void SubscribeEventEngine_OnEffectDispatch(IEffectInvocation obj)
        {
            try
            {
                unit?.EventTypeList.Add(new KeyValuePair<string, string>("invocation", obj?.Name));
                LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, EE OnEffectDispatch : CurrentState = {subscribeEventEngine.CurrentState.GetType().Name} => Invocation = {obj.GetType().Name}", config.LogVerbosity);
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, EE OnEffectDispatch : CurrentState = {subscribeEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}", config.LogVerbosity);
            }
        }

        private void SubscribeEventEngine_OnEventQueued(IEvent @event)
        {
            try
            {
                unit?.EventTypeList.Add(new KeyValuePair<string, string>("event", @event?.Name));
                int attempts = 0;
                if (subscribeEventEngine.CurrentState is HandshakeReconnectingState handshakeReconnectingState)
                {
                    attempts = handshakeReconnectingState.AttemptedRetries;
                }
                else if (subscribeEventEngine.CurrentState is ReceiveReconnectingState receiveReconnectingState)
                {
                    attempts = receiveReconnectingState.AttemptedRetries;
                }
                LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, EE OnEventQueued : CurrentState: {subscribeEventEngine.CurrentState.GetType().Name}; Event = {@event.GetType().Name}; Attempt = {attempts} of {config.ConnectionMaxRetries}", config.LogVerbosity);
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, EE OnEventQueued : CurrentState = {subscribeEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}", config.LogVerbosity);
            }
        }

        private void SubscribeEventEngine_OnStateTransition(EventEngine.Core.TransitionResult obj)
        {
            try
            {
                LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, EE OnStateTransition : CurrentState = {subscribeEventEngine.CurrentState.GetType().Name} => Transition State = {obj?.State.GetType().Name}", config.LogVerbosity);
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, EE OnStateTransition : CurrentState = {subscribeEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}", config.LogVerbosity);
            }
        }

		private void MessageEmitter<T>(Pubnub pubnubInstance, PNMessageResult<T> messageResult)
		{
			foreach (var listener in SubscribeListenerList)
            {
				listener?.Message(pubnubInstance, messageResult);
			}
		}

		private void StatusEmitter(Pubnub pubnubInstance, PNStatus status)
        {
            foreach (var listener in SubscribeListenerList)
            {
                listener?.Status(pubnubInstance, status);
            }
        }

	}
}
