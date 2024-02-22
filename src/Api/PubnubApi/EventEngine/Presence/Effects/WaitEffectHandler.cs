using System;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class WaitEffectHandler: EffectCancellableHandler<WaitInvocation, CancelWaitInvocation>
	{
		private Delay retryDelay = new(0);
		private EventQueue eventQueue;

		internal WaitEffectHandler(EventQueue eventQueue)
		{
			this.eventQueue = eventQueue;
		}

		public override async Task Cancel()
		{
            if (!retryDelay.Cancelled)
            {
                retryDelay.Cancel();
            }
		}

		public override bool IsBackground(WaitInvocation invocation)
		{
			return true;
		}

		public override async Task Run(WaitInvocation invocation)
		{
			retryDelay = new Delay((int)(invocation.HeartbeatInterval * 1000));
			await retryDelay.Start();
			if (!retryDelay.Cancelled)
			eventQueue.Enqueue(new Events.TimesUpEvent());
		}
	}
}

