using System;
using PubnubApi.EventEngine.Presence;
using PubnubApi.EventEngine.Presence.Events;

namespace PubnubApi.EndPoint
{
	public class PresenceOperation<T>
	{
		private PresenceEventEngineFactory presenceEventEngineFactory;
		private PresenceEventEngine presenceEventEngine;
		public PresenceOperation(Pubnub instance, string instanceId, IPubnubLog pubnubLog, TelemetryManager telemetryManager, TokenManager tokenManager, PresenceEventEngineFactory presenceEventEngineFactory)
		{
			this.presenceEventEngineFactory = presenceEventEngineFactory;
			if (this.presenceEventEngineFactory.HasEventEngine(instanceId)) {
				presenceEventEngine = this.presenceEventEngineFactory.GetEventEngine(instanceId);
			} else {
				presenceEventEngine = this.presenceEventEngineFactory.InitializeEventEngine<T>(instanceId, instance, pubnubLog, telemetryManager, tokenManager);
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

