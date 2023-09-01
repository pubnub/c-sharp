using System;
using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Context;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class HandshakingState : SubscriptionState
    {
        public override IEnumerable<IEffectInvocation> OnEntry => new HandshakeInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups, Cursor = this.Cursor }.AsArray();

        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.SubscriptionChangedEvent subscriptionChanged => new States.HandshakingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(subscriptionChanged.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(subscriptionChanged.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new States.HandshakingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.HandshakeFailureEvent handshakeFailure => new States.HandshakeReconnectingState()
                {
                    Channels = handshakeFailure.Channels,
                    ChannelGroups = handshakeFailure.ChannelGroups,
                    Cursor = handshakeFailure.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration,
                    AttemptedRetries = 1
                },

                Events.DisconnectEvent disconnect => new States.HandshakeStoppedState()
                {
                    Channels = disconnect.Channels,
                    ChannelGroups = disconnect.ChannelGroups,
                    Cursor = disconnect.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.HandshakeSuccessEvent success => new ReceivingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = success.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				}.With(new EmitStatusInvocation(success.Status)),

                _ => null
            };
        }
    }
}