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
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new States.HandshakingState()
                {
                    Channels = subscriptionChanged.Channels, ChannelGroups = subscriptionChanged.ChannelGroups
                }.With(),

                Events.SubscriptionRestoredEvent subscriptionRestored => new States.ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                }.With(),

                Events.HandshakeFailureEvent handshakeFailure => new States.HandshakeFailedState()
                {
                    Channels = this.Channels, ChannelGroups = this.ChannelGroups
                }.With(new EmitStatusInvocation(handshakeFailure.Status)),

                Events.DisconnectEvent disconnect => new States.HandshakeStoppedState()
                {
                    Channels = disconnect.Channels, ChannelGroups = disconnect.ChannelGroups,
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.HandshakeSuccessEvent success => new ReceivingState()
                {
                    Channels = this.Channels, ChannelGroups = this.ChannelGroups, Cursor = success.Cursor
                }.With(new EmitStatusInvocation(success.Status)),

                _ => null
            };
        }
    }
}