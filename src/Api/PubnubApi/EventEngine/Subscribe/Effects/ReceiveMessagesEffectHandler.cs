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


			if (resp.Item2 != null && !resp.Item2.Error && resp.Item1 != null) {
				// TODO how do we wire the messages to queue
				var c = new SubscriptionCursor() {
					Region = resp.Item1.Timetoken.Region,
					Timetoken = resp.Item1.Timetoken.Timestamp
				};
				
				eventQueue.Enqueue(new Events.ReceiveSuccessEvent() {Cursor = c});
			}
			else
			{
				PNStatus status;
				if (resp.Item2 is null)
				{
					status = new PNStatus()
					{
						Error = true,
						ErrorData = new PNErrorData("Unknown error.", new System.Exception("Unknown error")),
						Category = PNStatusCategory.PNUnknownCategory,
						AffectedChannels = invocation.Channels.ToList(),
						AffectedChannelGroups = invocation.ChannelGroups.ToList(),
					};
				}
				else
				{
                    status = resp.Item2;
				}
				eventQueue.Enqueue(new Events.ReceiveFailureEvent() { Status = status });
			}

		}

		public async Task Cancel() {
			manager.ReceiveRequestCancellation();
		}
	}
}
