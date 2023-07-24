﻿using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
    internal class HandshakingState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;

        public override IEnumerable<IEffectInvocation> OnEntry => new HandshakeInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups }.AsArray();

        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
                },

                Events.SubscriptionChangedEvent subscriptionChanged => new States.HandshakingState()
                {
                    Channels = subscriptionChanged.Channels, ChannelGroups = subscriptionChanged.ChannelGroups
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new States.ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                },

                Events.HandshakeFailureEvent handshakeFailure => new States.HandshakeReconnectingState()
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