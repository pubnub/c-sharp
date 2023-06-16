

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects {
	internal class HandshakeEffect : Core.IEffectHandler<HandshakeInvocation> {
		private SubscribeManager2 manager;
		private EventQueue eventQueue;

		public HandshakeEffect(SubscribeManager2 manager, EventQueue eventQueue) {
			this.manager = manager;
			this.eventQueue = eventQueue;
		}
		
		public async Task Run(HandshakeInvocation invocation) {
			// TODO fix this :)
			var resp = await manager.HandshakeRequest<string>(
				PNOperationType.PNSubscribeOperation, 
				invocation.channels.ToArray(), 
				invocation.channelGroups.ToArray(),
				invocation.cursor?.Timetoken,
				invocation.cursor?.Region,
				new Dictionary<string, string>(),
				new Dictionary<string, object>()
				);

			if (!resp.Item2.Error) {
				// eventQueue.Enqueue(new HandshakeSuccessEvent(...));
			}
		}

		public Task Cancel() {
			throw new System.NotImplementedException();
		}
	}
}