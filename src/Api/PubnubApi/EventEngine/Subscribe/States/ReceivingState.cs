using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class ReceivingState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public SubscriptionCursor Cursor;

        public IEnumerable<IEffectInvocation> OnEntry => new ReceiveMessagesInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups, Cursor = this.Cursor }.AsArray();

        public IEnumerable<IEffectInvocation> OnExit { get; } = new CancelReceiveMessagesInvocation().AsArray();

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            return e switch
            {
                Events.UnsubscribeAllEvent unsubscribeAll => new UnsubscribedState() 
                {
                    Channels = unsubscribeAll.Channels, ChannelGroups = unsubscribeAll.ChannelGroups,
                }.With(),

                Events.ReceiveSuccessEvent receiveSuccess => new ReceivingState()
                {
                    Channels = receiveSuccess.Channels,
                    ChannelGroups = receiveSuccess.ChannelGroups,
                    Cursor = receiveSuccess.Cursor
                }.With(
                    new EmitMessagesInvocation(receiveSuccess.Messages),
                    new EmitStatusInvocation(receiveSuccess.Status)
                ),

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    Cursor = this.Cursor
                }.With(),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                }.With(),

                Events.DisconnectEvent disconnect => new ReceiveStoppedState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = this.Cursor
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.ReceiveFailureEvent receiveFailure => new ReceiveReconnectingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = this.Cursor
                }.With(),

                _ => null
            };
        }
    }
}