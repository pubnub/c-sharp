﻿using System.Linq;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class UnsubscribedState : SubscriptionState
    {
        public override TransitionResult Transition(Core.IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels?? Enumerable.Empty<string>(),
                    ChannelGroups = subscriptionChanged.ChannelGroups??Enumerable.Empty<string>(),
                    Cursor = subscriptionChanged.Cursor,
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new States.HandshakingState()
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