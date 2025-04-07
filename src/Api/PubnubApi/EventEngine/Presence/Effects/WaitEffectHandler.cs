using System.Threading.Tasks;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class WaitEffectHandler : EffectCancellableHandler<WaitInvocation, CancelWaitInvocation>
	{
		private Delay retryDelay = new(0);
		private EventQueue eventQueue;
		private PNConfiguration pnConfiguration;

		internal WaitEffectHandler(PNConfiguration pnConfiguration, EventQueue eventQueue)
		{
			this.eventQueue = eventQueue;
			this.pnConfiguration = pnConfiguration;
		}

		public override async Task Cancel()
		{
			if (!retryDelay.Cancelled) {
				retryDelay.Cancel();
			}
		}

		public override bool IsBackground(WaitInvocation invocation)
		{
			return true;
		}

		public override async Task Run(WaitInvocation invocation)
		{
			retryDelay = new Delay((int)(pnConfiguration.PresenceInterval * 1000));
			await retryDelay.Start().ConfigureAwait(false);
			if (!retryDelay.Cancelled)
				eventQueue.Enqueue(new Events.TimesUpEvent());
		}
	}
}
