using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects
{
    internal class HandshakeEffectHandler : Core.IEffectHandler<HandshakeInvocation>
    {
        private SubscribeManager2 manager;
        private EventQueue eventQueue;

        public HandshakeEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
        {
            this.manager = manager;
            this.eventQueue = eventQueue;
        }

        public async Task Run(HandshakeInvocation invocation)
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

            if (resp.Item2 is null || resp.Item2.Error)
            {
                eventQueue.Enqueue(new Events.HandshakeFailureEvent() { Status = resp.Item2 });
                return;
            }

            HandshakeResponse handshakeResponse = null;

            try
            {
                // TODO move deserialization outside
                handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(resp.Item1);
                var c = new SubscriptionCursor()
                {
                    Region = handshakeResponse.Timetoken.Region,
                    Timetoken = handshakeResponse.Timetoken.Timestamp
                };

                eventQueue.Enqueue(new Events.HandshakeSuccessEvent() { Cursor = c });
            }
            catch (System.Exception e)
            {
                eventQueue.Enqueue(new Events.HandshakeFailureEvent()
                    {
                        Status = new PNStatus(e, PNOperationType.PNSubscribeOperation,
                            PNStatusCategory.PNUnknownCategory, invocation.Channels, invocation.ChannelGroups)
                    }
                );
            }
        }

        public async Task Cancel()
        {
            manager.HandshakeRequestCancellation();
        }
    }
}