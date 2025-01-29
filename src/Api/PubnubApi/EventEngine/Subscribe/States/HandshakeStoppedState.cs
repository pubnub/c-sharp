using System.Linq;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
	public class HandshakeStoppedState : SubscriptionState
    {
        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() { }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels?? Enumerable.Empty<string>(),
                    ChannelGroups = subscriptionChanged.ChannelGroups??Enumerable.Empty<string>(),
                }.With(
                    new EmitStatusInvocation(new PNStatus(null,PNOperationType.PNSubscribeOperation, PNStatusCategory.PNSubscriptionChangedCategory, this.Channels, this.ChannelGroups, Constants.HttpRequestSuccessStatusCode))),

                Events.ReconnectEvent reconnect => new HandshakingState()
                {
                    Channels = reconnect.Channels,
                    ChannelGroups = reconnect.ChannelGroups,
                    Cursor = reconnect.Cursor,
                    
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new HandshakingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
                    
                },

                _ => null
            };
        }
    }
}