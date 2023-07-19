﻿using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
    internal class HandshakeFailedState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels, ChannelGroups = subscriptionChanged.ChannelGroups,
                },

                Events.ReconnectEvent reconnect => new HandshakingState()
                {
                    Channels = reconnect.Channels, ChannelGroups = reconnect.ChannelGroups,
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                },
                
                _ => null
            };
        }
    }
}