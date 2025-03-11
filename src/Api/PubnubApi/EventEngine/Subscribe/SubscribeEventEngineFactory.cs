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
			engineInstances = new ConcurrentDictionary<string, SubscribeEventEngine>();
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
			IJsonPluggableLibrary jsonPluggableLibrary,
			Action<Pubnub, PNStatus> statusListener = null)
		{
			var subscribeEventEngine = new SubscribeEventEngine(pubnubInstance, pubnubConfiguration: pubnubConfiguration, subscribeManager,eventEmitter, jsonPluggableLibrary, statusListener);
			try
			{
				engineInstances.TryAdd(instanceId, subscribeEventEngine);
				return subscribeEventEngine;
			}
			catch (Exception e)
			{
				pubnubConfiguration.Logger.Error($"Subscribe Event engine initialisation failed due to exception {e.Message} \n {e.StackTrace}");
				throw new Exception("Subscribe event engine initialisation failed!");
			}
		}
	}
}

