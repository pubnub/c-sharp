using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects
{
    internal class HandshakeEffectHandler : 
        Core.IEffectHandler<HandshakeInvocation>,
        Core.IEffectHandler<HandshakeReconnectInvocation>
    {
        private SubscribeManager2 manager;
        private EventQueue eventQueue;

        public HandshakeEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public async Task Run(HandshakeReconnectInvocation invocation)
        {
            await this.Run((HandshakeInvocation)invocation);
        }

        public async Task Run(HandshakeInvocation invocation)
        {
            var response = await MakeHandshakeRequest(invocation);

            switch (invocation)
            {
                case Invocations.HandshakeReconnectInvocation reconnectInvocation when response.Item2.Error:
                    eventQueue.Enqueue(new Events.HandshakeReconnectFailureEvent() {RemainingRetries = reconnectInvocation.RemainingRetries - 1, Status = response.Item2});
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

        public async Task Cancel()
        {
            manager.HandshakeRequestCancellation();
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
    }
}