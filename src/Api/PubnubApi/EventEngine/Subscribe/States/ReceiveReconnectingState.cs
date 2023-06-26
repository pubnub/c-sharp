using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class ReceiveReconnectingState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public SubscriptionCursor Cursor;

        public IEnumerable<IEffectInvocation> OnEntry => new ReceiveReconnectInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups, Cursor = this.Cursor }.AsArray();

        public IEnumerable<IEffectInvocation> OnExit { get; } =
            new CancelReceiveReconnectInvocation().AsArray();

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new ReceivingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                        Cursor = this.Cursor
                    }.With(
                        new ReceiveMessagesInvocation()
                        {
                            Channels = subscriptionChanged.Channels,
                            ChannelGroups = subscriptionChanged.ChannelGroups,
                            Cursor = this.Cursor
                        }
                    );
                case Events.DisconnectEvent disconnect:
                    return new ReceiveStoppedState()
                    {
                        Channels = disconnect.Channels,
                        ChannelGroups = disconnect.ChannelGroups,
                        Cursor = disconnect.Cursor
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Channels = disconnect.Channels,
                            ChannelGroups = disconnect.ChannelGroups,
                            StatusCategory = PNStatusCategory.PNDisconnectedCategory
                        }
                    );
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new ReceivingState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups,
                        Cursor = subscriptionRestored.Cursor
                    }.With(null);
                case Events.ReceiveReconnectSuccessEvent receiveReconnectSuccess:
                    return new ReceivingState()
                    {
                        Channels = receiveReconnectSuccess.Channels,
                        ChannelGroups = receiveReconnectSuccess.ChannelGroups,
                        Cursor = receiveReconnectSuccess.Cursor
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Channels = receiveReconnectSuccess.Channels,
                            ChannelGroups = receiveReconnectSuccess.ChannelGroups,
                            StatusCategory = PNStatusCategory.PNReconnectedCategory
                        }
                    );
                case Events.ReceiveReconnectFailureEvent receiveReconnectFailure:
                    return new ReceiveReconnectingState()
                    {
                        Channels = receiveReconnectFailure.Channels,
                        ChannelGroups = receiveReconnectFailure.ChannelGroups,
                        Cursor = receiveReconnectFailure.Cursor
                    }.With(null);
                case Events.ReceiveReconnectGiveUpEvent receiveReconnectGiveUp:
                    return new ReceiveFailedState()
                    {
                        Channels = receiveReconnectGiveUp.Channels,
                        ChannelGroups = receiveReconnectGiveUp.ChannelGroups,
                        Cursor = receiveReconnectGiveUp.Cursor
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Status = receiveReconnectGiveUp.Status
                        }
                    );
                default: return null;
            }
        }
    }
}