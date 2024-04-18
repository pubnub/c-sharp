using System;
using System.Collections.Concurrent;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Common;

namespace PubnubApi.EventEngine.Subscribe
{
	public class SubscribeEventEngineFactory
	{
		private ConcurrentDictionary<string, SubscribeEventEngine> engineInstances { get; set;}
		internal SubscribeEventEngineFactory()
		{ 
			this.engineInstances = new ConcurrentDictionary<string, SubscribeEventEngine>();
		}
		internal bool HasEventEngine(string instanceId)
		{
			return engineInstances.ContainsKey(instanceId);
		}
		internal SubscribeEventEngine GetEventEngine(string instanceId)
		{
			SubscribeEventEngine subscribeEventEngine;
			engineInstances.TryGetValue(instanceId, out subscribeEventEngine);
			return subscribeEventEngine;
		}

		internal SubscribeEventEngine InitializeEventEngine(string instanceId,
			Pubnub pubnubInstance,
			PNConfiguration pubnubConfiguration,
			SubscribeManager2 subscribeManager,
			EventEmitter eventEmitter,
			Action<Pubnub, PNStatus> statusListener = null)
		{
			
			var subscribeEventEngine = new SubscribeEventEngine(pubnubInstance, pubnubConfiguration: pubnubConfiguration, subscribeManager,eventEmitter, statusListener);
			if (engineInstances.TryAdd(instanceId, subscribeEventEngine)) {
				return subscribeEventEngine;
			}
			else {
				throw new Exception("Subscribe event engine initialisation failed!");
			}
		}
	}
}

