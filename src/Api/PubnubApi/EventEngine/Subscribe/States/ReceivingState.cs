﻿using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using System.Linq;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class ReceivingState : SubscriptionState
    {
        public override IEnumerable<IEffectInvocation> OnEntry => new ReceiveMessagesInvocation()
            { Channels = this.Channels,ChannelGroups = this.ChannelGroups, Cursor = this.Cursor }.AsArray();

        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelReceiveMessagesInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.ReceiveSuccessEvent receiveSuccess => new ReceivingState()
                {
                    Channels = receiveSuccess.Channels,
                    ChannelGroups = receiveSuccess.ChannelGroups,
                    Cursor = receiveSuccess.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(
                    new EmitMessagesInvocation(receiveSuccess.Cursor, receiveSuccess.Messages)
                ),

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(subscriptionChanged.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(subscriptionChanged.ChannelGroups),
                    Cursor = subscriptionChanged.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.DisconnectEvent disconnect => new ReceiveStoppedState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = disconnect.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.ReceiveFailureEvent receiveFailure => new ReceiveReconnectingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = receiveFailure.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration,
                    AttemptedRetries = 1
                },

                _ => null
            };
        }
    }
}