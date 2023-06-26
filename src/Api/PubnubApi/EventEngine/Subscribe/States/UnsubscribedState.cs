using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class UnsubscribedState : Core.IState
    {
        public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; }

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(Core.IEvent e)
        {
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new HandshakingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                    }.With();
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new States.ReceivingState()
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