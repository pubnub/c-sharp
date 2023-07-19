using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;
using PubnubApi.PubnubEventEngine.Subscribe.Events;
using PubnubApi.PubnubEventEngine;
using System;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	internal class HandshakeReconnectEffectHandler : IEffectHandler<HandshakeReconnectInvocation>
	{
		private SubscribeManager2 manager;
		private EventQueue eventQueue;

		public HandshakeReconnectEffectHandler(SubscribeManager2 manager, EventQueue eventQueue)
		{
			this.manager = manager;
			this.eventQueue = eventQueue;
		}

		public async Task Cancel()
		{
			manager.HandshakeRequestCancellation();
		}

		public async Task IEffectHandler<HandshakeReconnectInvocation>.Run(HandshakeReconnectInvocation invocation)
		{
			if (!ReconnectionDelayUtil.shouldRetry(invocation.Policy, invocation.AttemptedRetries, invocation.MaxConnectionRetry))
			{
				eventQueue.Enqueue(new HandshakeReconnectGiveUpEvent() { Status = PNStatus(PNStatusCategory.PNCancelledCategory) });
			}
			else
			{
				// TODO: Check cancellation token whether this operation is cancelled already or not.

				// have a delay - blocking/ non-blocking thing
				// TODO:   Look for examples in PCL library
				await Task.Delay(ReconnectionDelayUtil.CalculateDelay(invocation.Policy, invocation.AttemptedRetries));

				// TODO: Check cancellation token whether this operation is cancelled or not.
				// attempt handshake
				var handshakeResponse = await attemptHandshake(invocation);

				if (handshakeResponse.Item2.Error) {
					eventQueue.Enqueue(new HandshakeReconnectFailureEvent() { AttemptedRetries = invocation.AttemptedRetries + 1, Status = handshakeResponse.Item2 });
				}

				eventQueue.Enqueue(new HandshakeReconnectSuccessEvent() { Cursor = handshakeResponse.Item1, Status = handshakeResponse.Item2 });
			}

		}

		private async Task<System.Tuple<SubscriptionCursor, PNStatus>> attemptHandshake(HandshakeReconnectInvocation invocation)
		{
			var resp = await manager.HandshakeRequest<string>(
				PNOperationType.PNSubscribeOperation,
				invocation.Channels.ToArray(),
				invocation.ChannelGroups.ToArray(),
				null,
				null,
				invocation.InitialSubscribeQueryParams,
				invocation.ExternalQueryParams
			);

			try {
				var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(resp.Item1);
				var c = new SubscriptionCursor() {
					Region = handshakeResponse.Timetoken.Region,
					Timetoken = handshakeResponse.Timetoken.Timestamp
				};
				return new System.Tuple<SubscriptionCursor, PNStatus>(c, resp.Item2);
			} catch (Exception e) {
				return new Tuple<SubscriptionCursor, PNStatus>(null, new PNStatus(e, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnknownCategory, invocation.Channels, invocation.ChannelGroups));
			}
		}
	}

}

