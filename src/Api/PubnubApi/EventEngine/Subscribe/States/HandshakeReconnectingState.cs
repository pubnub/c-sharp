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
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new HandshakingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                    }.With(null);
                case Events.DisconnectEvent disconnect:
                    return new HandshakeStoppedState()
                    {
                        Channels = disconnect.Channels,
                        ChannelGroups = disconnect.ChannelGroups
                    }.With(null);
                case Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUp:
                    return new HandshakeFailedState()
                    {
                        Channels = handshakeReconnectGiveUp.Channels,
                        ChannelGroups = handshakeReconnectGiveUp.ChannelGroups
                    }.With();
                case Events.HandshakeReconnectSuccessEvent handshakeReconnectSuccess:
                    return new ReceivingState()
                    {
                        Channels = handshakeReconnectSuccess.Channels,
                        ChannelGroups = handshakeReconnectSuccess.ChannelGroups,
                        Cursor = handshakeReconnectSuccess.Cursor
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Channels = handshakeReconnectSuccess.Channels,
                            ChannelGroups = handshakeReconnectSuccess.ChannelGroups,
                            StatusCategory = PNStatusCategory.PNReconnectedCategory
                        }
                    );
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new HandshakeFailedState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups
                    }.With();
                default: return null;
            }
        }
    }
}