using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Context;
using System.Linq;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class ReceiveFailedState : SubscriptionState
    {
        public override IEnumerable<IEffectInvocation> OnEntry { get; }
        public override IEnumerable<IEffectInvocation> OnExit { get; }

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(subscriptionChanged.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(subscriptionChanged.ChannelGroups),
                    Cursor = this.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.ReconnectEvent reconnect => new HandshakingState()
                {
                    Channels = reconnect.Channels,
                    ChannelGroups = reconnect.ChannelGroups,
                    Cursor = reconnect.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.SubscriptionRestoredEvent subscriptionRestored => new HandshakingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                _ => null
            };
        }
    }
}