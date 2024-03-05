using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.States
{
	public class HandshakeReconnectingState : SubscriptionState
	{
		public int AttemptedRetries;

		public override IEnumerable<IEffectInvocation> OnEntry => new HandshakeReconnectInvocation() {
			Channels = this.Channels,
			ChannelGroups = this.ChannelGroups,
			AttemptedRetries = this.AttemptedRetries
		}.AsArray();
		public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHandshakeReconnectInvocation().AsArray();

		public override TransitionResult Transition(IEvent e)
		{
			return e switch {
				Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() {
				},

				Events.SubscriptionChangedEvent subscriptionChanged => new HandshakingState() {
					Channels = subscriptionChanged.Channels,
					ChannelGroups = subscriptionChanged.ChannelGroups,
				},

				Events.DisconnectEvent disconnect => new HandshakeStoppedState() {
					Channels = disconnect.Channels,
					ChannelGroups = disconnect.ChannelGroups,
				},

				Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUp => new HandshakeFailedState() {
					Channels = this.Channels,
					ChannelGroups = this.ChannelGroups,
				}.With(
					new EmitStatusInvocation(handshakeReconnectGiveUp.Status)
				),

				Events.HandshakeReconnectFailureEvent handshakeReconnectFailure => new HandshakeReconnectingState() {
					Channels = this.Channels,
					ChannelGroups = this.ChannelGroups,
					AttemptedRetries = this.AttemptedRetries + 1
				}.With(new EmitStatusInvocation(handshakeReconnectFailure.Status)),

				Events.HandshakeReconnectSuccessEvent handshakeReconnectSuccess => new ReceivingState() {
					Channels = this.Channels,
					ChannelGroups = this.ChannelGroups,
					Cursor = handshakeReconnectSuccess.Cursor,

				}.With(new EmitStatusInvocation(handshakeReconnectSuccess.Status)),

				Events.SubscriptionRestoredEvent subscriptionRestored => new HandshakingState() {
					Channels = subscriptionRestored.Channels,
					ChannelGroups = subscriptionRestored.ChannelGroups,
					Cursor = subscriptionRestored.Cursor,

				},

				_ => null
			};
		}
	}
}