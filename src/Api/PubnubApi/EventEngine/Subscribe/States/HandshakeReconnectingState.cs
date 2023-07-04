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

        public IEnumerable<IEffectInvocation> OnEntry => new HandshakeReconnectInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups }.AsArray();
        public IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeReconnectInvocation().AsArray();

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
                    Channels = unsubscribeAll.Channels, ChannelGroups = unsubscribeAll.ChannelGroups,
                }.With(),

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
                }.With(),

                _ => null
            };
        }
    }
}