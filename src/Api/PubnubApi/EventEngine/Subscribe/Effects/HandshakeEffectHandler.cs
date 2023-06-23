using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects {
	internal class HandshakeEffectHandler : Core.IEffectHandler<HandshakeInvocation> {
		private SubscribeManager2 manager;
		private EventQueue eventQueue;

		public HandshakeEffectHandler(SubscribeManager2 manager, EventQueue eventQueue) {
			this.manager = manager;
			this.eventQueue = eventQueue;
		}

		public async Task Run(HandshakeInvocation invocation) {
			// TODO fix this, probably wrong :)
			var resp = await manager.HandshakeRequest<string>(
				PNOperationType.PNSubscribeOperation,
				invocation.Channels.ToArray(),
				invocation.ChannelGroups.ToArray(),
				null,
				null,
				invocation.InitialSubscribeQueryParams,
				invocation.ExternalQueryParams
			);

			if (!resp.Item2.Error) {
				// TODO move deserialization outside
				// TODO does this need more error checking?
				var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(resp.Item1);
				var c = new SubscriptionCursor() {
					Region = handshakeResponse.Timetoken.Region,
					Timetoken = handshakeResponse.Timetoken.Timestamp
				};
				
				eventQueue.Enqueue(new Events.HandshakeSuccessEvent() {cursor = c});
			}
		}

		public async Task Cancel() {
			manager.HandshakeRequestCancellation();
		}
	}
}