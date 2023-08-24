using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using System.Linq;

namespace PubnubApi.EventEngine.Subscribe.States
{
    public class ReceiveReconnectingState : SubscriptionState
    {
        public int AttemptedRetries { get; set;}

		public override IEnumerable<IEffectInvocation> OnEntry => new ReceiveReconnectInvocation()
        {
            Channels = this.Channels,
            ChannelGroups = this.ChannelGroups,
            Cursor = this.Cursor,
            ReconnectionConfiguration = this.ReconnectionConfiguration,
            AttemptedRetries = this.AttemptedRetries
        }.AsArray();

        public override IEnumerable<IEffectInvocation> OnExit { get; } =
            new CancelReceiveReconnectInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(subscriptionChanged.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(subscriptionChanged.ChannelGroups),
                    Cursor = this.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.DisconnectEvent disconnect => new ReceiveStoppedState()
                {
                    Channels = disconnect.Channels,
                    ChannelGroups = disconnect.ChannelGroups,
                    Cursor = disconnect.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.ReceiveReconnectSuccessEvent receiveReconnectSuccess => new ReceivingState()
                {
                    Channels = receiveReconnectSuccess.Channels,
                    ChannelGroups = receiveReconnectSuccess.ChannelGroups,
                    Cursor = receiveReconnectSuccess.Cursor,
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(
                    new EmitMessagesInvocation(receiveReconnectSuccess.Cursor, receiveReconnectSuccess.Messages)
                ),

                Events.ReceiveReconnectFailureEvent receiveReconnectFailure => new ReceiveReconnectingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = this.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration,
                    AttemptedRetries = (this.AttemptedRetries + 1) % int.MaxValue
				}.With(new EmitStatusInvocation(receiveReconnectFailure.Status)),

                Events.ReceiveReconnectGiveUpEvent receiveReconnectGiveUp => new ReceiveFailedState()
                {
                    Channels = receiveReconnectGiveUp.Channels,
                    ChannelGroups = receiveReconnectGiveUp.ChannelGroups,
                    Cursor = receiveReconnectGiveUp.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				}.With(new EmitStatusInvocation(receiveReconnectGiveUp.Status)),

                _ => null
            };
        }
    }
}