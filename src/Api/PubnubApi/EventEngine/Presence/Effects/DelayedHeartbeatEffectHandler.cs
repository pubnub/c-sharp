﻿using System;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class DelayedHeartbeatEffectHandler : EffectCancellableHandler<DelayedHeartbeatInvocation, CancelDelayedHeartbeatInvocation>
	{
		private SubscribeManager2 manager;
		private EventQueue eventQueue;
		private Delay retryDelay = new Delay(0);

		internal DelayedHeartbeatEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
		{
			this.manager = manager;
			this.eventQueue = eventQueue;
		}
		public override bool IsBackground(DelayedHeartbeatInvocation invocation) => true;
		public override async Task Run(DelayedHeartbeatInvocation invocation)
		{

			if (!ReconnectionDelayUtil.shouldRetry(invocation.ReconnectionConfiguration, invocation.AttemptedRetries))
			{
				eventQueue.Enqueue(new Events.HeartbeatGiveUpEvent() { Status = new PNStatus(PNStatusCategory.PNCancelledCategory) });
			}
			else
			{
				retryDelay = new Delay(ReconnectionDelayUtil.CalculateDelay(invocation.ReconnectionConfiguration.ReconnectionPolicy, invocation.AttemptedRetries));
				await retryDelay.Start();
				if (!retryDelay.Cancelled)
					await MakeHeartbeatRequest(invocation);
			}

			
		}

		private async Task MakeHeartbeatRequest(DelayedHeartbeatInvocation invocation)
		{
			var resp = await manager.HeartbeatRequest<string>(
				PNOperationType.PNHeartbeatOperation,
				invocation.Input.Channels.ToArray(),
				invocation.Input.ChannelGroups.ToArray()
			);
			switch (resp)
			{
				case { } when resp.Error:
					eventQueue.Enqueue(new Events.HeartbeatFailureEvent() { AttemptedRetries = invocation.AttemptedRetries + 1, Status = resp });
					break;
				case { }:
					eventQueue.Enqueue(new Events.HeartbeatSuccessEvent());
					break;
			}
		}

		public override async Task Cancel()
		{
            if (!retryDelay.Cancelled)
            {
                retryDelay.Cancel();
            }
		}
	}
}
