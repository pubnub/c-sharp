﻿using System.Linq;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.States
{
	public class ReceiveStoppedState : SubscriptionState
    {
        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() { },

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceiveStoppedState()
                {
                    Channels = subscriptionChanged.Channels?? Enumerable.Empty<string>(),
                    ChannelGroups = subscriptionChanged.ChannelGroups??Enumerable.Empty<string>(),
                    Cursor = this.Cursor,
                },
                
                Events.ReconnectEvent reconnect => new HandshakingState()
                {
                    Channels = reconnect.Channels,
                    ChannelGroups = reconnect.ChannelGroups,
                    Cursor = reconnect.Cursor,
                },
                
                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceiveStoppedState()
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