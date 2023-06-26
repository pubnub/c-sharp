using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class HandshakeFailedState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;

        public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; }

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new HandshakingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                    }.With();
                case Events.ReconnectEvent reconnect:
                    return new HandshakingState()
                    {
                        Channels = reconnect.Channels,
                        ChannelGroups = reconnect.ChannelGroups,
                    }.With();
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new ReceivingState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups,
                        Cursor = subscriptionRestored.Cursor
                    }.With();
                default: return null;
            }
        }
    }
}