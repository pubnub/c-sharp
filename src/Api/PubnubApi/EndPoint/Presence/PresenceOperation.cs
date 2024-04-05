using System;
using System.Collections.Generic;
using System.Globalization;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence;
using PubnubApi.EventEngine.Presence.Events;

namespace PubnubApi.EndPoint
{
	public class PresenceOperation<T>
	{
		private PresenceEventEngineFactory presenceEventEngineFactory;
		private PNConfiguration configuration;
		private PresenceEventEngine presenceEventEngine;
		private IPubnubUnitTest unit;
		private IPubnubLog pubnubLog;

		public PresenceOperation(Pubnub instance, string instanceId, IPubnubLog pubnubLog, PNConfiguration configuration, TelemetryManager telemetryManager, TokenManager tokenManager, IPubnubUnitTest unit, PresenceEventEngineFactory presenceEventEngineFactory)
		{
			this.pubnubLog = pubnubLog;
			this.unit = unit;
			this.configuration = configuration;
			this.presenceEventEngineFactory = presenceEventEngineFactory;
			if (unit != null) { unit.PresenceActivityList = new List<KeyValuePair<string, string>>(); }

			if (this.presenceEventEngineFactory.HasEventEngine(instanceId)) {
				presenceEventEngine = this.presenceEventEngineFactory.GetEventEngine(instanceId);
			} else {
				presenceEventEngine = this.presenceEventEngineFactory.InitializeEventEngine<T>(instanceId, instance, pubnubLog, telemetryManager, tokenManager);
				presenceEventEngine.OnEffectDispatch += OnEffectDispatch;
				presenceEventEngine.OnEventQueued += OnEventQueued;
			}
		}

		private void OnEventQueued(IEvent e)
		{
			try {
				unit?.PresenceActivityList.Add(new KeyValuePair<string, string>("event", e?.Name));
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, presemce ee OnEventQueued : CurrentState = {presenceEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}", configuration.LogVerbosity);
			}
		}

		private void OnEffectDispatch(IEffectInvocation invocation)
		{
			try {
				unit?.PresenceActivityList.Add(new KeyValuePair<string, string>("invocation", invocation?.Name));
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, presence ee OnEffectDispatch : CurrentState = {presenceEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}", configuration.LogVerbosity);
			}
		}

		public void Start(string[] channels, string[] channelGroups)
		{
			this.presenceEventEngine.EventQueue.Enqueue(new JoinedEvent() {
				Input = new EventEngine.Presence.Common.PresenceInput() { Channels = channels, ChannelGroups = channelGroups }
			});
		}
	}
}

