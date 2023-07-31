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
        Core.IEffectHandler<HandshakeInvocation>,
        Core.IEffectHandler<HandshakeReconnectInvocation>
    {
        private SubscribeManager2 manager;
        private EventQueue eventQueue;
        
        private Delay retryDelay = new Delay(0);

        internal HandshakeEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public async Task Run(HandshakeReconnectInvocation invocation)
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
                    await Run((HandshakeInvocation)invocation);
            }
        }

        public bool IsBackground(HandshakeReconnectInvocation invocation)
        {
            return true;
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

        public bool IsBackground(HandshakeInvocation invocation)
        {
            return false;
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