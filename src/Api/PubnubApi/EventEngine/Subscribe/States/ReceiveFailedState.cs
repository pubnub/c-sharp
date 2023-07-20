using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.States
{
    internal class ReceiveFailedState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public SubscriptionCursor Cursor;
		public PNReconnectionPolicy ReconnectionPolicy;
		public int MaximumReconnectionRetries;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; }

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    Cursor = this.Cursor,
					MaximumReconnectionRetries = this.MaximumReconnectionRetries,
					ReconnectionPolicy = this.ReconnectionPolicy
				},

                Events.ReconnectEvent reconnect => new ReceivingState()
                {
                    Channels = reconnect.Channels,
                    ChannelGroups = reconnect.ChannelGroups,
                    Cursor = reconnect.Cursor,
					MaximumReconnectionRetries = this.MaximumReconnectionRetries,
					ReconnectionPolicy = this.ReconnectionPolicy
				},

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
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