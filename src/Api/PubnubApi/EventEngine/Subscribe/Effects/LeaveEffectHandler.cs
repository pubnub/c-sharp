using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects {
	internal class LeaveEffectHandler : Core.IEffectHandler<LeaveInvocation> {
		private SubscribeManager2 manager;
		private EventQueue eventQueue;

		public LeaveEffectHandler(SubscribeManager2 manager, EventQueue eventQueue) {
			this.manager = manager;
			this.eventQueue = eventQueue;
		}

		public async Task Run(LeaveInvocation invocation) {
			// TODO identity method for unsubscribe all
			//var resp = null;//await manager.UnsubscribeRequest<string>(
			//	PNOperationType.PNSubscribeOperation,
			//	invocation.Channels.ToArray(),
			//	invocation.ChannelGroups.ToArray(),
			//);

			
		}

		public async Task Cancel() {
			//manager.LeaveRequestCancellation();
		}
	}
}
