using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
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
					
				},

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(subscriptionChanged.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(subscriptionChanged.ChannelGroups),
                    Cursor = this.Cursor,
                },

                Events.DisconnectEvent disconnect => new ReceiveStoppedState()
                {
                    Channels = disconnect.Channels,
                    ChannelGroups = disconnect.ChannelGroups,
                    Cursor = disconnect.Cursor,
                    
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor,
                    
                },

                Events.ReceiveReconnectSuccessEvent receiveReconnectSuccess => new ReceivingState()
                {
                    Channels = receiveReconnectSuccess.Channels,
                    ChannelGroups = receiveReconnectSuccess.ChannelGroups,
                    Cursor = receiveReconnectSuccess.Cursor,
                    
                }.With(
                    new EmitMessagesInvocation(receiveReconnectSuccess.Cursor, receiveReconnectSuccess.Messages)
                ),

                Events.ReceiveReconnectFailureEvent receiveReconnectFailure => new ReceiveReconnectingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = this.Cursor,
                    AttemptedRetries = this.AttemptedRetries + 1
				},

                Events.ReceiveReconnectGiveUpEvent receiveReconnectGiveUp => new ReceiveFailedState()
                {
                    Channels = receiveReconnectGiveUp.Channels,
                    ChannelGroups = receiveReconnectGiveUp.ChannelGroups,
                    Cursor = receiveReconnectGiveUp.Cursor,
					
				}.With(new EmitStatusInvocation(receiveReconnectGiveUp.Status)),

                _ => null
            };
        }
    }
}