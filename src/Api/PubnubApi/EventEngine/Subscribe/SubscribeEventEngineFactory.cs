using System;
using System.Collections.Concurrent;
using PubnubApi.EndPoint;

namespace PubnubApi.EventEngine.Subscribe
{
	public class SubscribeEventEngineFactory
	{
		private ConcurrentDictionary<string, SubscribeEventEngine> engineinstances;
		internal SubscribeEventEngineFactory()
		{ 
			this.engineinstances = new ConcurrentDictionary<string, SubscribeEventEngine>();
		}
		internal bool hasEventEngine(string instanceId)
		{
			return engineinstances.ContainsKey(instanceId);
		}
		internal SubscribeEventEngine getEventEngine(string instanceId)
		{
			SubscribeEventEngine subscribeEventEngine;
			engineinstances.TryGetValue(instanceId, out subscribeEventEngine);
			return subscribeEventEngine;
		}

		internal SubscribeEventEngine initializeEventEngine<T>(string instanceId,
			Pubnub pubnubInstance,
			PNConfiguration pubnubConfiguration,
			SubscribeManager2 subscribeManager,
			Action<Pubnub, PNStatus> statusListener = null,
			Action<Pubnub, PNMessageResult<T>> messageListener= null)
		{
			var subscribeEventEngine = new SubscribeEventEngine(pubnubInstance, pubnubConfiguration: pubnubConfiguration, subscribeManager,statusListener, null); //TODO: replace with message listener
			if (engineinstances.TryAdd(instanceId, subscribeEventEngine)) {
				return subscribeEventEngine;
			}
			else {
				throw new Exception("Subscribe event engine initialisation failed!");
			}
			
		}
	}
}

