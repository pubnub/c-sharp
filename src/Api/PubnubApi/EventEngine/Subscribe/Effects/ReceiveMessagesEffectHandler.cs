using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects
{
    internal class ReceiveMessagesEffectHandler: Core.IEffectHandler<ReceiveMessagesInvocation>
    {
        private SubscribeManager2 manager;
		private EventQueue eventQueue;

        public ReceiveMessagesEffectHandler(SubscribeManager2 manager, EventQueue eventQueue) {
			this.manager = manager;
			this.eventQueue = eventQueue;
		}

		public async Task Run(ReceiveMessagesInvocation invocation) {
			// TODO get ReceiveingResponse as response from ReceiveRequest
			var resp = await manager.ReceiveRequest<string>(
				PNOperationType.PNSubscribeOperation,
				invocation.Channels.ToArray(),
				invocation.ChannelGroups.ToArray(),
				invocation.Cursor.Timetoken,
				invocation.Cursor.Region,
				null,
				invocation.ExternalQueryParams
			);

			if (!resp.Item2.Error) {
				// TODO does this need more error checking?
				// TODO how do we wire the messages to queue
				var c = new SubscriptionCursor() {
					Region = resp.Item1.Timetoken.Region,
					Timetoken = resp.Item1.Timetoken.Timestamp
				};
				
				eventQueue.Enqueue(new Events.ReceiveSuccessEvent() {Cursor = c});
			}
		}

		public async Task Cancel() {
			manager.ReceiveRequestCancellation();
		}
	}
}
