using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;
using System;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
    public class ReceivingEffectHandler:
        EffectCancellableHandler<ReceiveMessagesInvocation, CancelReceiveMessagesInvocation>
    {
        private SubscribeManager2 manager;
        private EventQueue eventQueue;

        internal ReceivingEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public override async Task Run(ReceiveMessagesInvocation invocation)
        {
            var response = await MakeReceiveMessagesRequest(invocation);
            SubscriptionCursor cursor = null;
            if (response.Item1 is not null) {
                cursor = new SubscriptionCursor() {
                    Region = response.Item1.Timetoken.Region,
                    Timetoken = response.Item1.Timetoken.Timestamp
                };
            }

            if (response.Item2 is null)
                return;

            switch (invocation)
            {
                case ReceiveReconnectInvocation reconnectInvocation when response.Item2.Error:
                    eventQueue.Enqueue(new ReceiveReconnectFailureEvent() { AttemptedRetries = (reconnectInvocation.AttemptedRetries + 1) % int.MaxValue, Status = response.Item2});
                    break;
                case ReceiveReconnectInvocation reconnectInvocation:
                    eventQueue.Enqueue(new ReceiveReconnectSuccessEvent() { Channels = invocation?.Channels, ChannelGroups = invocation?.ChannelGroups, Cursor = cursor, Status = response.Item2, Messages = response.Item1 });
                    break;
                case { } when response.Item2.Error:
                    eventQueue.Enqueue(new ReceiveFailureEvent() { Cursor = invocation.Cursor, Status = response.Item2, AttemptedRetries = 0});
                    break;
                case { }:
                    eventQueue.Enqueue(new ReceiveSuccessEvent() { Channels = invocation?.Channels, ChannelGroups = invocation?.ChannelGroups, Cursor = cursor, Messages= response.Item1, Status = response.Item2 });
                    break;
            }
        }

        public override bool IsBackground(ReceiveMessagesInvocation invocation)
        {
            return true;
        }

        private async Task<Tuple<ReceivingResponse<object>, PNStatus>> MakeReceiveMessagesRequest(ReceiveMessagesInvocation invocation)
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
            manager.ReceiveRequestCancellation();
        }
    }

    public class ReceivingReconnectEffectHandler :
        EffectCancellableHandler<ReceiveReconnectInvocation, CancelReceiveReconnectInvocation>
    {
        private PNConfiguration pubnubConfiguration;
        private EventQueue eventQueue;
        private ReceivingEffectHandler receivingEffectHandler;
        
        private Delay retryDelay = new Delay(0);   
        
        internal ReceivingReconnectEffectHandler(PNConfiguration pubnubConfiguration,  EventQueue eventQueue, ReceivingEffectHandler receivingEffectHandler)
        {
            this.pubnubConfiguration = pubnubConfiguration;
            this.eventQueue = eventQueue;
            this.receivingEffectHandler = receivingEffectHandler;
        }
        
        public override async Task Run(ReceiveReconnectInvocation invocation)
        {
            var retryConfiguration = pubnubConfiguration.RetryConfiguration;
			if (retryConfiguration == null)
            {
                eventQueue.Enqueue(new ReceiveReconnectGiveUpEvent() { Status = new PNStatus(new Exception(""), PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, invocation.Channels, invocation.ChannelGroups, invocation.Reason.StatusCode ) });
            }
			else if (!retryConfiguration.RetryPolicy.ShouldRetry(invocation.AttemptedRetries, invocation.Reason))
            {
				eventQueue.Enqueue(new ReceiveReconnectGiveUpEvent() { Status = new PNStatus(new Exception(""), PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, invocation.Channels, invocation.ChannelGroups, invocation.Reason.StatusCode ) });
			}
            else
            {
                retryDelay = new Delay(retryConfiguration.RetryPolicy.GetDelay(invocation.AttemptedRetries, invocation.Reason, null));
                // Run in the background
                await retryDelay.Start();
                await receivingEffectHandler.Run(invocation);
            }
        }
        
        public override bool IsBackground(ReceiveReconnectInvocation invocation)
        {
            return true;
        }
        
        public override async Task Cancel()
        {
            if (!retryDelay.Cancelled)
            {
                retryDelay.Cancel();
            }
            await receivingEffectHandler.Cancel();
        }
    }
}
