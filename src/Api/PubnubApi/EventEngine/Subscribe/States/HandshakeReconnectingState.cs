using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class HandshakeReconnectingState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public PNReconnectionPolicy RetryPolicy;
        public int MaxConnectionRetry;
        public int AttemptedRetries;

        public override IEnumerable<IEffectInvocation> OnEntry => new HandshakeReconnectInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups, Policy = this.RetryPolicy, MaxConnectionRetry = this.MaxConnectionRetry, AttemptedRetries = this.AttemptedRetries }.AsArray();
        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeReconnectInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
                },

                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels, ChannelGroups = subscriptionChanged.ChannelGroups,
                },

                Events.DisconnectEvent disconnect => new HandshakeStoppedState()
                {
                    Channels = disconnect.Channels, ChannelGroups = disconnect.ChannelGroups
                },

                Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUp => new HandshakeFailedState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups
                }.With(
                    new EmitStatusInvocation(handshakeReconnectGiveUp.Status)
                ),

                Events.HandshakeReconnectSuccessEvent handshakeReconnectSuccess => new ReceivingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = handshakeReconnectSuccess.Cursor
                }.With(new EmitStatusInvocation(handshakeReconnectSuccess.Status)),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                },

                Events.HandshakeReconnectFailureEvent handshakeReconnectFailed => new HandshakeReconnectingState()
                {
                    Channels = handshakeReconnectFailed.Channels, ChannelGroups = handshakeReconnectFailed.ChannelGroups
                },

                _ => null
            };
        }
    }
}