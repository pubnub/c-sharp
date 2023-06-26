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
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new HandshakingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                    }.With(null);
                case Events.ReconnectEvent reconnect:
                    return new HandshakingState()
                    {
                        Channels = reconnect.Channels,
                        ChannelGroups = reconnect.ChannelGroups,
                    }.With(null);
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new ReceivingState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups,
                        Cursor = subscriptionRestored.Cursor
                    }.With(null);
                case Events.HandshakeFailureEvent handshakeFailure:
                    return new HandshakeReconnectingState()
                    {
                        Channels = this.Channels,
                        ChannelGroups = this.ChannelGroups,
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Status = handshakeFailure.Status
                        }
                    );
                case Events.DisconnectEvent disconnect:
                    return new HandshakeStoppedState()
                    {
                        Channels = disconnect.Channels,
                        ChannelGroups = disconnect.ChannelGroups,
                    }.With(
                        new EmitStatusInvocation()
                        {
                            StatusCategory = PNStatusCategory.PNDisconnectedCategory
                        }
                    );
                case Events.HandshakeSuccessEvent handshakeSuccess:
                    return new ReceivingState()
                    {
                        Channels = this.Channels,
                        ChannelGroups = this.ChannelGroups,
                        Cursor = handshakeSuccess.cursor
                    }.With(
                        new EmitStatusInvocation()
                        {
                            StatusCategory = PNStatusCategory.PNConnectedCategory
                        }
                    );
                default: return null;
            }
        }
    }
}