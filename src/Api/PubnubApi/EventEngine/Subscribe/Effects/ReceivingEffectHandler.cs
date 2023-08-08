using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
    public class ReceivingEffectHandler:
        EffectDoubleCancellableHandler<ReceiveMessagesInvocation, ReceiveReconnectInvocation, CancelReceiveMessagesInvocation, CancelReceiveReconnectInvocation>
    {
        private SubscribeManager2 manager;
        private EventQueue eventQueue;
        
        private Delay retryDelay = new Delay(0);

        internal ReceivingEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public override Task Run(ReceiveReconnectInvocation invocation)
        {
            if (!ReconnectionDelayUtil.shouldRetry(invocation.ReconnectionConfiguration, invocation.AttemptedRetries))
            {
                eventQueue.Enqueue(new ReceiveReconnectGiveUpEvent() { Status = new PNStatus(PNStatusCategory.PNCancelledCategory) });
            }
            else
            {
                retryDelay = new Delay(ReconnectionDelayUtil.CalculateDelay(invocation.ReconnectionConfiguration.ReconnectionPolicy, invocation.AttemptedRetries));
                // Run in the background
                retryDelay.Start().ContinueWith((_) => this.Run((ReceiveMessagesInvocation)invocation));
            }

            return Utils.EmptyTask;
        }

        public override bool IsBackground(ReceiveReconnectInvocation invocation)
        {
            return true;
        }

        public override async Task Run(ReceiveMessagesInvocation invocation)
        {
            var response = await MakeReceiveMessagesRequest(invocation);
            var cursor = new SubscriptionCursor()
            {
                Region = response.Item1?.Timetoken.Region,
                Timetoken = response.Item1?.Timetoken.Timestamp
            };

            // Assume that if status is null, the effect was cancelled.
            if (response.Item2 is null)
                return;

            switch (invocation)
            {
                case Invocations.ReceiveReconnectInvocation reconnectInvocation when response.Item2.Error:
                    eventQueue.Enqueue(new Events.ReceiveReconnectFailureEvent() { AttemptedRetries = reconnectInvocation.AttemptedRetries + 1, Status = response.Item2});
                    break;
                case Invocations.ReceiveReconnectInvocation reconnectInvocation:
                    eventQueue.Enqueue(new Events.ReceiveReconnectSuccessEvent() { Channels = invocation?.Channels, ChannelGroups = invocation?.ChannelGroups, Cursor = cursor, Status = response.Item2 });
                    break;
                case { } when response.Item2.Error:
                    eventQueue.Enqueue(new Events.ReceiveFailureEvent() { Cursor = invocation.Cursor, Status = response.Item2});
                    break;
                case { }:
                    eventQueue.Enqueue(new Events.ReceiveSuccessEvent() { Channels = invocation?.Channels, ChannelGroups = invocation?.ChannelGroups, Cursor = cursor, Messages= response.Item1, Status = response.Item2 });
                    break;
            }
        }

        public override bool IsBackground(ReceiveMessagesInvocation invocation)
        {
            return true;
        }

        private async Task<System.Tuple<ReceivingResponse<object>, PNStatus>> MakeReceiveMessagesRequest(ReceiveMessagesInvocation invocation)
        {
            return await manager.ReceiveRequest<ReceivingResponse<object>>(
                PNOperationType.PNSubscribeOperation,
                invocation.Channels?.ToArray(),
                invocation.ChannelGroups?.ToArray(),
                invocation.Cursor.Timetoken.Value,
                invocation.Cursor.Region.Value,
                invocation.InitialSubscribeQueryParams,
                invocation.ExternalQueryParams
            );
        }

        public override async Task Cancel()
        {
            if (!retryDelay.Cancelled)
            {
                retryDelay.Cancel();
            }
            else
            {
                manager.ReceiveRequestCancellation();
            }
        }
    }
}
