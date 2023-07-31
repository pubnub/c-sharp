using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class HandshakeFailedState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
		public ReconnectionConfiguration ReconnectionConfiguration;

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
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.ReconnectEvent reconnect => new HandshakingState()
                {
                    Channels = reconnect.Channels,
                    ChannelGroups = reconnect.ChannelGroups,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
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