using System;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class HeartbeatEffectHandler: EffectHandler<Invocations.HeartbeatInvocation>
	{
		private SubscribeManager2 manager;
		private EventQueue eventQueue;

		internal HeartbeatEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
		{
			this.manager = manager;
			this.eventQueue = eventQueue;
		}

		public override bool IsBackground(HeartbeatInvocation invocation) => false;

		public override async Task Run(HeartbeatInvocation invocation)
		{
            var resp = await manager.HeartbeatRequest<string>(
                PNOperationType.PNHeartbeatOperation,
                invocation.Input.Channels.ToArray(),
                invocation.Input.ChannelGroups.ToArray()
            );
			switch (resp) {
				case { } when resp.Error:
					eventQueue.Enqueue(new Events.HeartbeatFailureEvent() { AttemptedRetries = 0,Status = resp});
					break;
				case { }:
					eventQueue.Enqueue(new Events.HeartbeatSuccessEvent());
					break;
			}
		}

		public override Task Cancel()
		{
			throw new NotImplementedException();
		}
	}
}

