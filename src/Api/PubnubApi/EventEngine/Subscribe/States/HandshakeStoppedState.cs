using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class HandshakeStoppedState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;

        public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; }

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels, ChannelGroups = subscriptionChanged.ChannelGroups,
                }.With(null),

                Events.ReconnectEvent reconnect => new HandshakingState()
                {
                    Channels = reconnect.Channels, ChannelGroups = reconnect.ChannelGroups,
                }.With(null),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                }.With(null),

                Events.HandshakeFailureEvent handshakeFailure => new HandshakeReconnectingState()
                {
                    Channels = this.Channels, ChannelGroups = this.ChannelGroups,
                }.With(new EmitStatusInvocation() { Status = handshakeFailure.Status }),

                Events.DisconnectEvent disconnect => new HandshakeStoppedState()
                {
                    Channels = disconnect.Channels, ChannelGroups = disconnect.ChannelGroups,
                }.With(new EmitStatusInvocation() { StatusCategory = PNStatusCategory.PNDisconnectedCategory }),

                Events.HandshakeSuccessEvent handshakeSuccess => new ReceivingState()
                {
                    Channels = this.Channels, ChannelGroups = this.ChannelGroups, Cursor = handshakeSuccess.cursor
                }.With(new EmitStatusInvocation() { StatusCategory = PNStatusCategory.PNConnectedCategory }),

                _ => null
            };
        }
    }
}