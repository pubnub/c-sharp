using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class ReceivingState : SubscriptionState
    {
        public SubscriptionCursor Cursor;

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
                    new EmitMessagesInvocation(receiveSuccess.Messages),
                    new EmitStatusInvocation(receiveSuccess.Status)
                ),

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    Cursor = this.Cursor,
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
                    Cursor = this.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.ReceiveFailureEvent receiveFailure => new ReceiveReconnectingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = this.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration,
                    AttemptedRetries = 0
                },

                _ => null
            };
        }
    }
}