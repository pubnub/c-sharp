using System;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class HeartbeatEffectHandler : EffectHandler<Invocations.HeartbeatInvocation>
	{
		private HertbeatOperation heartbeatOperation;
		private EventQueue eventQueue;

		internal HeartbeatEffectHandler(HertbeatOperation heartbeatOperation, EventQueue eventQueue)
		{
			this.heartbeatOperation = heartbeatOperation;
			this.eventQueue = eventQueue;
		}

		public override bool IsBackground(HeartbeatInvocation invocation) => false;

		public override async Task Run(HeartbeatInvocation invocation)
		{
			var resp = await heartbeatOperation.HeartbeatRequest<string>(
				invocation.Input.Channels.ToArray(),
				invocation.Input.ChannelGroups.ToArray()
			);
			switch (resp) {
				case { } when resp.Error:
					eventQueue.Enqueue(new Events.HeartbeatFailureEvent() { AttemptedRetries = 0, Status = resp });
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

