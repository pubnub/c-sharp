using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class HandshakingState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;

        public IEnumerable<IEffectInvocation> OnEntry => new HandshakeInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups }.AsArray();

        public IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeInvocation().AsArray();

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new States.HandshakingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups
                    }.With();
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new States.ReceivingState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups,
                        Cursor = subscriptionRestored.Cursor
                    }.With();
                case Events.HandshakeFailureEvent handshakeFailure:
                    return new States.HandshakeFailedState()
                    {
                        Channels = this.Channels,
                        ChannelGroups = this.ChannelGroups
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Status = handshakeFailure.Status
                        }
                    );
                case Events.DisconnectEvent disconnect:
                    return new States.HandshakeStoppedState()
                    {
                        Channels = disconnect.Channels,
                        ChannelGroups = disconnect.ChannelGroups,
                    }.With(
                        new EmitStatusInvocation()
                        {
                            StatusCategory = PNStatusCategory.PNDisconnectedCategory
                        }
                    );
                case Events.HandshakeSuccessEvent success:
                    return new ReceivingState()
                    {
                        Channels = this.Channels,
                        ChannelGroups = this.ChannelGroups,
                        Cursor = success.cursor
                    }.With(new EmitStatusInvocation()
                    {
                        StatusCategory = PNStatusCategory.PNConnectedCategory
                    });
                default: return null;
            }
        }
    }
}