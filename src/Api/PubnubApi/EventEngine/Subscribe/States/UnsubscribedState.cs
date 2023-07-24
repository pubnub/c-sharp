using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
    internal class UnsubscribedState : Core.State
    {
		public PNReconnectionPolicy ReconnectionPolicy;
		public int MaximumReconnectionRetries;

		public override TransitionResult Transition(Core.IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
					MaximumReconnectionRetries = this.MaximumReconnectionRetries,
					ReconnectionPolicy = this.ReconnectionPolicy
				},

                Events.SubscriptionRestoredEvent subscriptionRestored => new States.ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
					MaximumReconnectionRetries = this.MaximumReconnectionRetries,
					ReconnectionPolicy = this.ReconnectionPolicy
				},

                _ => null
            };
        }
    }
}