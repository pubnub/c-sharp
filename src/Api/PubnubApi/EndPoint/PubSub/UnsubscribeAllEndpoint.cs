using PubnubApi.EventEngine.Presence;
using PubnubApi.EventEngine.Subscribe;

namespace PubnubApi.EndPoint
{
	public class UnsubscribeAllEndpoint<T>: UnsubscribeAllOperation<T>
	{
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TokenManager pubnubTokenManager;
        private readonly SubscribeEventEngineFactory subscribeEventEngineFactory;
        private readonly PresenceEventEngineFactory presenceEventEngineFactory;
        private readonly string instanceId;

		public UnsubscribeAllEndpoint(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager,  SubscribeEventEngineFactory subscribeEventEngineFactory, PresenceEventEngineFactory presenceEventEngineFactory ,Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTokenManager = tokenManager;
            this.subscribeEventEngineFactory = subscribeEventEngineFactory;
            this.presenceEventEngineFactory = presenceEventEngineFactory;
            instanceId = PubnubInstance.InstanceId;
            UnsubscribeAll();
        }


        private void UnsubscribeAll()
        {
			if (subscribeEventEngineFactory.HasEventEngine(instanceId)) {
				SubscribeEventEngine subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(instanceId);
                subscribeEventEngine.UnsubscribeAll();
			}

            if (config.PresenceInterval > 0 && presenceEventEngineFactory.HasEventEngine(instanceId)) {
                PresenceEventEngine presenceEventEngine = presenceEventEngineFactory.GetEventEngine(instanceId);
                presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.LeftAllEvent());
            }

            if (config.MaintainPresenceState) {
                if (ChannelLocalUserState.TryGetValue(instanceId, out var channelStates)) {
                    channelStates.Clear();
                }
                if (ChannelLocalUserState.TryGetValue(instanceId, out var channelGroupStates)) {
                    channelGroupStates.Clear();
                }
            }
        }
	}
}

