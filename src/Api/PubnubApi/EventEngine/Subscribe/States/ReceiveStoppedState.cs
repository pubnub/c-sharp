using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class ReceiveStoppedState : SubscriptionState
    {
        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceiveStoppedState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
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
                
                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceiveStoppedState()
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