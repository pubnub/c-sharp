using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class HandshakeReconnectingState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        
        // TODO introduce policy provider
        public int RetryCount = 3;

        public override IEnumerable<IEffectInvocation> OnEntry => new HandshakeReconnectInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups, RemainingRetries = this.RetryCount}.AsArray();
        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeReconnectInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
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

                _ => null
            };
        }
    }
}