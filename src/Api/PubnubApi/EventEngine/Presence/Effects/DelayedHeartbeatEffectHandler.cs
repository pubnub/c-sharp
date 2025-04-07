using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class DelayedHeartbeatEffectHandler : EffectCancellableHandler<DelayedHeartbeatInvocation, CancelDelayedHeartbeatInvocation>
	{
		private PNConfiguration pubnubConfiguration;
		private HeartbeatOperation heartbeatOperation;
		private EventQueue eventQueue;
		private Delay retryDelay = new Delay(0);

		internal DelayedHeartbeatEffectHandler(PNConfiguration pubnubConfiguration, HeartbeatOperation heartbeatOperation, EventQueue eventQueue)
		{
			this.pubnubConfiguration = pubnubConfiguration;
			this.heartbeatOperation = heartbeatOperation;
			this.eventQueue = eventQueue;
		}
		public override bool IsBackground(DelayedHeartbeatInvocation invocation) => true;
		public override async Task Run(DelayedHeartbeatInvocation invocation)
		{
			var retryConfiguration = pubnubConfiguration.RetryConfiguration;
			if (retryConfiguration == null) {
				EnqueueHeartbeatGiveUpEvent();
				return;
			}

			if (!retryConfiguration.RetryPolicy.ShouldRetry(invocation.RetryCount, invocation.Reason)) {
				EnqueueHeartbeatGiveUpEvent();
				return;
			}
			retryDelay = new Delay(retryConfiguration.RetryPolicy.GetDelay(invocation.RetryCount, invocation.Reason, null));
			await retryDelay.Start().ConfigureAwait(false);
			if (!retryDelay.Cancelled)
				await MakeHeartbeatRequest(invocation).ConfigureAwait(false);
		}

		private void EnqueueHeartbeatGiveUpEvent()
		{
			eventQueue.Enqueue(new Events.HeartbeatGiveUpEvent() { Status = new PNStatus(PNStatusCategory.PNUnexpectedDisconnectCategory) });
		}

		private async Task MakeHeartbeatRequest(DelayedHeartbeatInvocation invocation)
		{
			var resp = await heartbeatOperation.HeartbeatRequest<string>(
				invocation.Input.Channels.ToArray(),
				invocation.Input.ChannelGroups.ToArray()
			).ConfigureAwait(false);
			switch (resp) {
				case { } when resp.Error:
					eventQueue.Enqueue(new Events.HeartbeatFailureEvent() { retryCount = invocation.RetryCount + 1, Status = resp });
					break;
				case { }:
					eventQueue.Enqueue(new Events.HeartbeatSuccessEvent());
					break;
			}
		}

		public override async Task Cancel()
		{
			if (!retryDelay.Cancelled) {
				retryDelay.Cancel();
			}
		}
	}
}
