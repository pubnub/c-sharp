using System;
using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
	public class HandshakeReconnectingState : SubscriptionState
	{
		public int AttemptedRetries;

		public override IEnumerable<IEffectInvocation> OnEntry => new HandshakeReconnectInvocation()
		{
			Channels = this.Channels,
			ChannelGroups = this.ChannelGroups,
			ReconnectionConfiguration = this.ReconnectionConfiguration,
			AttemptedRetries = this.AttemptedRetries
		}.AsArray();
		public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeReconnectInvocation().AsArray();

		public override TransitionResult Transition(IEvent e)
		{
			return e switch
			{
				Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState()
				{
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

				Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState()
				{
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(subscriptionChanged.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(subscriptionChanged.ChannelGroups),
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

				Events.DisconnectEvent disconnect => new HandshakeStoppedState()
				{
					Channels = disconnect.Channels,
					ChannelGroups = disconnect.ChannelGroups,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

				Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUp => new HandshakeFailedState()
				{
					Channels = this.Channels,
					ChannelGroups = this.ChannelGroups,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				}.With(
					new EmitStatusInvocation(handshakeReconnectGiveUp.Status)
				),

				Events.HandshakeReconnectFailureEvent handshakeReconnectFailure => new HandshakeReconnectingState()
				{
					Channels = this.Channels,
					ChannelGroups = this.ChannelGroups,
					ReconnectionConfiguration = this.ReconnectionConfiguration,
					AttemptedRetries = (this.AttemptedRetries + 1) % int.MaxValue
				}.With(new EmitStatusInvocation(handshakeReconnectFailure.Status)),

				Events.HandshakeReconnectSuccessEvent handshakeReconnectSuccess => new ReceivingState()
				{
					Channels = this.Channels,
					ChannelGroups = this.ChannelGroups,
					Cursor = handshakeReconnectSuccess.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				}.With(new EmitStatusInvocation(handshakeReconnectSuccess.Status)),

				Events.SubscriptionRestoredEvent subscriptionRestored => new HandshakingState()
				{
					Channels = subscriptionRestored.Channels,
					ChannelGroups = subscriptionRestored.ChannelGroups,
					Cursor = subscriptionRestored.Cursor,
					ReconnectionConfiguration = this.ReconnectionConfiguration
				},

				_ => null
			};
		}
	}
}