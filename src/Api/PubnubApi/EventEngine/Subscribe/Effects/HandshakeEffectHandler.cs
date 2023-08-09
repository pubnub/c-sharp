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
    public class HandshakeEffectHandler : 
        EffectCancellableHandler<HandshakeInvocation, CancelHandshakeInvocation>
    {
        private readonly SubscribeManager2 manager;
        private readonly EventQueue eventQueue;

        internal HandshakeEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public override async Task Run(HandshakeInvocation invocation)
        {
            var response = await MakeHandshakeRequest(invocation);
            SubscriptionCursor cursor = null;
            if (response.Item1 != null)
            {
                cursor = new SubscriptionCursor()
                {
                    Region = response.Item1.Timetoken.Region,
                    Timetoken = response.Item1.Timetoken.Timestamp
                };
            }

            switch (invocation)
            {
                case Invocations.HandshakeReconnectInvocation reconnectInvocation when response.Item2.Error:
                    eventQueue.Enqueue(new Events.HandshakeReconnectFailureEvent() { AttemptedRetries = reconnectInvocation.AttemptedRetries + 1, Status = response.Item2});
                    break;
                case Invocations.HandshakeReconnectInvocation reconnectInvocation:
                    eventQueue.Enqueue(new Events.HandshakeReconnectSuccessEvent() { Cursor = cursor, Status = response.Item2 });
                    break;
                case { } when response.Item2.Error:
                    eventQueue.Enqueue(new Events.HandshakeFailureEvent() { Status = response.Item2});
                    break;
                case { }:
                    eventQueue.Enqueue(new Events.HandshakeSuccessEvent() { Cursor = cursor, Status = response.Item2 });
                    break;
                
            }
        }

        public override bool IsBackground(HandshakeInvocation invocation)
        {
            return false;
        }


        private async Task<System.Tuple<HandshakeResponse, PNStatus>> MakeHandshakeRequest(HandshakeInvocation invocation)
        {
            return await manager.HandshakeRequest(
                PNOperationType.PNSubscribeOperation,
                invocation.Channels.ToArray(),
                invocation.ChannelGroups.ToArray(),
                null,
                null,
                invocation.InitialSubscribeQueryParams,
                invocation.ExternalQueryParams
            );
        }

        public override async Task Cancel()
        {
            manager.HandshakeRequestCancellation();
        }

    }

    public class HandshakeReconnectEffectHandler : EffectCancellableHandler<HandshakeReconnectInvocation, CancelHandshakeReconnectInvocation>
    {
        private readonly EventQueue eventQueue;

        private HandshakeEffectHandler handshakeEffectHandler;
        
        private Delay retryDelay = new Delay(0);
      
        
        internal HandshakeReconnectEffectHandler(SubscribeManager2 manager, EventQueue eventQueue, HandshakeEffectHandler handshakeEffectHandler)
        {
            this.eventQueue = eventQueue;
            this.handshakeEffectHandler = handshakeEffectHandler;
        }

        public override async Task Run(HandshakeReconnectInvocation invocation)
        {
            if (!ReconnectionDelayUtil.shouldRetry(invocation.ReconnectionConfiguration, invocation.AttemptedRetries))
            {
                eventQueue.Enqueue(new HandshakeReconnectGiveUpEvent() { Status = new PNStatus(PNStatusCategory.PNCancelledCategory) });
            }
            else
            {
                retryDelay = new Delay(ReconnectionDelayUtil.CalculateDelay(invocation.ReconnectionConfiguration.ReconnectionPolicy, invocation.AttemptedRetries));
                await retryDelay.Start();
                if (!retryDelay.Cancelled)
                    await handshakeEffectHandler.Run(invocation as HandshakeInvocation);
            }
        }

        public override bool IsBackground(HandshakeReconnectInvocation invocation) => true;
        
        public override async Task Cancel()
        {
            retryDelay.Cancel();
            await handshakeEffectHandler.Cancel();
        }
    }
}