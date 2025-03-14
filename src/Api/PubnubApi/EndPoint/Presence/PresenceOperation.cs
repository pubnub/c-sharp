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

		public PresenceOperation(Pubnub instance, string instanceId, PNConfiguration configuration, TokenManager tokenManager, IPubnubUnitTest unit, PresenceEventEngineFactory presenceEventEngineFactory)
		{
			this.unit = unit;
			this.configuration = configuration;
			this.presenceEventEngineFactory = presenceEventEngineFactory;
			if (unit != null) { unit.PresenceActivityList = new List<KeyValuePair<string, string>>(); }

			if (this.presenceEventEngineFactory.HasEventEngine(instanceId)) {
				presenceEventEngine = this.presenceEventEngineFactory.GetEventEngine(instanceId);
			} else {
				presenceEventEngine = this.presenceEventEngineFactory.InitializeEventEngine<T>(instanceId, instance, tokenManager);
				presenceEventEngine.OnEffectDispatch += OnEffectDispatch;
				presenceEventEngine.OnEventQueued += OnEventQueued;
			}
		}

		private void OnEventQueued(IEvent e)
		{
			try {
				unit?.PresenceActivityList.Add(new KeyValuePair<string, string>("event", e?.Name));
			} catch (Exception ex)
			{
				configuration.Logger?.Error(
					$"presence event engine OnEventQueued : CurrentState = {presenceEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}");
			}
		}

		private void OnEffectDispatch(IEffectInvocation invocation)
		{
			try {
				unit?.PresenceActivityList.Add(new KeyValuePair<string, string>("invocation", invocation?.Name));
			} catch (Exception ex)
			{
				configuration.Logger?.Error(
					$"presence event engine OnEffectDispatch : CurrentState = {presenceEventEngine.CurrentState.GetType().Name} => EXCEPTION = {ex}");
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

