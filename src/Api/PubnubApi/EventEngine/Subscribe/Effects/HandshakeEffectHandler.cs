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
    internal class HandshakeEffectHandler : 
        Core.IEffectHandler<HandshakeInvocation>,
        Core.IEffectHandler<HandshakeReconnectInvocation>
    {
        private SubscribeManager2 manager;
        private EventQueue eventQueue;
        
        private Delay retryDelay = new Delay(0);

        public HandshakeEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public Task Run(HandshakeReconnectInvocation invocation)
        {
            if (!ReconnectionDelayUtil.shouldRetry(invocation.Policy, invocation.AttemptedRetries, invocation.MaxConnectionRetry))
            {
                eventQueue.Enqueue(new HandshakeReconnectGiveUpEvent() { Status = new PNStatus(PNStatusCategory.PNCancelledCategory) });
            }
            else
            {
                retryDelay = new Delay(ReconnectionDelayUtil.CalculateDelay(invocation.Policy, invocation.AttemptedRetries));
                // Run in the background
                retryDelay.Start().ContinueWith((_) => this.Run((HandshakeInvocation)invocation));
            }

            return Utils.EmptyTask;
        }

        public async Task Run(HandshakeInvocation invocation)
        {
            var response = await MakeHandshakeRequest(invocation);

            switch (invocation)
            {
                case Invocations.HandshakeReconnectInvocation reconnectInvocation when response.Item2.Error:
                    eventQueue.Enqueue(new Events.HandshakeReconnectFailureEvent() { AttemptedRetries = reconnectInvocation.AttemptedRetries + 1, Status = response.Item2});
                    break;
                case Invocations.HandshakeReconnectInvocation reconnectInvocation:
                    eventQueue.Enqueue(new Events.HandshakeReconnectSuccessEvent() { Cursor = response.Item1, Status = response.Item2 });
                    break;
                case { } when response.Item2.Error:
                    eventQueue.Enqueue(new Events.HandshakeFailureEvent() { Status = response.Item2});
                    break;
                case { }:
                    eventQueue.Enqueue(new Events.HandshakeSuccessEvent() { Cursor = response.Item1, Status = response.Item2 });
                    break;
                
            }
        }
        
        
        private async Task<System.Tuple<SubscriptionCursor, PNStatus>> MakeHandshakeRequest(HandshakeInvocation invocation)
        {
            var resp = await manager.HandshakeRequest<string>(
                PNOperationType.PNSubscribeOperation,
                invocation.Channels.ToArray(),
                invocation.ChannelGroups.ToArray(),
                null,
                null,
                invocation.InitialSubscribeQueryParams,
                invocation.ExternalQueryParams
            );

            try
            {
                var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(resp.Item1);
                var c = new SubscriptionCursor()
                {
                    Region = handshakeResponse.Timetoken.Region,
                    Timetoken = handshakeResponse.Timetoken.Timestamp
                };
                return new System.Tuple<SubscriptionCursor, PNStatus>(c, resp.Item2);
            }
            catch (Exception e)
            {
                return new Tuple<SubscriptionCursor, PNStatus>(null, new PNStatus(e, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnknownCategory, invocation.Channels, invocation.ChannelGroups));
            }
        }

        public async Task Cancel()
        {
            if (!retryDelay.Cancelled)
            {
                retryDelay.Cancel();
            }
            else
            {
                manager.HandshakeRequestCancellation();
            }
        }

    }
}