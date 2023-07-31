using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class ReceiveFailedState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public SubscriptionCursor Cursor;
		public ReconnectionConfiguration ReconnectionConfiguration;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; }

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    Cursor = this.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.ReconnectEvent reconnect => new ReceivingState()
                {
                    Channels = reconnect.Channels,
                    ChannelGroups = reconnect.ChannelGroups,
                    Cursor = reconnect.Cursor,
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