using System;
using System.Collections.Concurrent;
using PubnubApi.EndPoint;

namespace PubnubApi.EventEngine.Presence
{
	public class PresenceEventEngineFactory
	{
		private ConcurrentDictionary<string, PresenceEventEngine> engineInstances;
		public PresenceEventEngineFactory()
		{
			this.engineInstances = new ConcurrentDictionary<string, PresenceEventEngine>();
		}

		internal bool HasEventEngine(string instanceId)
		{
			return engineInstances.ContainsKey(instanceId);
		}

		internal PresenceEventEngine GetEventEngine(string instanceId)
		{
			PresenceEventEngine subscribeEventEngine;
			engineInstances.TryGetValue(instanceId, out subscribeEventEngine);
			return subscribeEventEngine;
		}

		internal PresenceEventEngine InitializeEventEngine<T>(string instanceId,
			Pubnub pubnubInstance, TokenManager tokenManager)
		{
			HeartbeatOperation heartbeatOperation = new HeartbeatOperation(pubnubInstance.PNConfig, pubnubInstance.JsonPluggableLibrary, pubnubInstance.PubnubUnitTest, tokenManager, pubnubInstance);
			LeaveOperation leaveOperation = new LeaveOperation(pubnubInstance.PNConfig, pubnubInstance.JsonPluggableLibrary, pubnubInstance.PubnubUnitTest, tokenManager, pubnubInstance);
			var presenceEventEngine = new PresenceEventEngine(pubnubInstance.PNConfig, heartbeatOperation, leaveOperation);
			if (engineInstances.TryAdd(instanceId, presenceEventEngine)) {
				return presenceEventEngine;
			} else {
				throw new Exception("PresenceEventEngine initialisation failed!");
			}

		}
	}
}
