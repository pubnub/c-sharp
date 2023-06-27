using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class HandshakeReconnectingState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;

        public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeReconnectInvocation().AsArray();

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
                {
                    Channels = subscriptionChanged.Channels, ChannelGroups = subscriptionChanged.ChannelGroups,
                }.With(),

                Events.DisconnectEvent disconnect => new HandshakeStoppedState()
                {
                    Channels = disconnect.Channels, ChannelGroups = disconnect.ChannelGroups
                }.With(),

                Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUp => new HandshakeFailedState()
                {
                    Channels = handshakeReconnectGiveUp.Channels,
                    ChannelGroups = handshakeReconnectGiveUp.ChannelGroups
                }.With(),

                Events.HandshakeReconnectSuccessEvent handshakeReconnectSuccess => new ReceivingState()
                {
                    Channels = handshakeReconnectSuccess.Channels,
                    ChannelGroups = handshakeReconnectSuccess.ChannelGroups,
                    Cursor = handshakeReconnectSuccess.Cursor
                }.With(new EmitStatusInvocation()
                {
                    Channels = handshakeReconnectSuccess.Channels,
                    ChannelGroups = handshakeReconnectSuccess.ChannelGroups,
                    StatusCategory = PNStatusCategory.PNReconnectedCategory
                }),

                Events.SubscriptionRestoredEvent subscriptionRestored => new HandshakeFailedState()
                {
                    Channels = subscriptionRestored.Channels, ChannelGroups = subscriptionRestored.ChannelGroups
                }.With(),

                _ => null
            };
        }
    }
}