using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Presence;
using PubnubApi.EventEngine.Subscribe;

namespace PubnubApi.EndPoint
{
	public class UnsubscribeAllEndpoint<T>: UnsubscribeAllOperation<T>
	{
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly SubscribeEventEngineFactory subscribeEventEngineFactory;
        private readonly PresenceEventEngineFactory presenceEventEngineFactory;
        private readonly LeaveOperation leaveOperation;
        private readonly string instanceId;

		public UnsubscribeAllEndpoint(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, LeaveOperation leaveOperation, SubscribeEventEngineFactory subscribeEventEngineFactory, PresenceEventEngineFactory presenceEventEngineFactory ,Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance, false)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            this.leaveOperation = leaveOperation;
            this.subscribeEventEngineFactory = subscribeEventEngineFactory;
            this.presenceEventEngineFactory = presenceEventEngineFactory;
            instanceId = PubnubInstance.InstanceId;
            UnsubscribeAll();
        }


        private void UnsubscribeAll()
        {
            logger?.Trace($"{GetType().Name} Execute invoked");

            var subscribeEventEngines = new List<SubscribeEventEngine>();
            if (config.SplitSubscribeCalls)
            {
                subscribeEventEngines = subscribeEventEngineFactory.GetAllEngineInstances(instanceId);
            }
            else if (subscribeEventEngineFactory.HasEventEngine(instanceId))
            {
                logger?.Trace($"EventEngine instance found.");
                subscribeEventEngines.Add(subscribeEventEngineFactory.GetEventEngine(instanceId));
            }

            bool usePresenceEventEngine = config.PresenceInterval > 0 && presenceEventEngineFactory.HasEventEngine(instanceId);
            foreach (var subscribeEventEngine in subscribeEventEngines)
            {
                var channelsToLeave = subscribeEventEngine.Channels.Where(c => !c.EndsWith(Constants.Pnpres)).Distinct().ToArray();
                var channelGroupsToLeave = subscribeEventEngine.ChannelGroups.Where(cg => !cg.EndsWith(Constants.Pnpres)).Distinct().ToArray();

                subscribeEventEngine.UnsubscribeAll();

                if (!usePresenceEventEngine && !config.SuppressLeaveEvents && (channelsToLeave.Length > 0 || channelGroupsToLeave.Length > 0))
                {
                    SendLeave(channelsToLeave, channelGroupsToLeave);
                }
            }

            if (usePresenceEventEngine) {
                PresenceEventEngine presenceEventEngine = presenceEventEngineFactory.GetEventEngine(instanceId);
                presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.LeftAllEvent());
            }

            if (config.MaintainPresenceState) {
                if (ChannelLocalUserState.TryGetValue(instanceId, out var channelStates)) {
                    channelStates.Clear();
                }
                if (ChannelGroupLocalUserState.TryGetValue(instanceId, out var channelGroupStates)) {
                    channelGroupStates.Clear();
                }
            }
        }

        private void SendLeave(string[] channels, string[] channelGroups)
        {
            leaveOperation.LeaveRequest<string>(channels, channelGroups).ContinueWith(t =>
            {
                if (t.Result is { Error: true } status)
                {
                    logger?.Warn($"UnsubscribeAll leave request failed for channel(s)={string.Join(",", channels)} channelGroup(s)={string.Join(",", channelGroups)}: {status.ErrorData?.Information}");
                }
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion);
        }
	}
}
