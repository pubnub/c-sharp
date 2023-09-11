using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Invocations;

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
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new States.ReceivingState()
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