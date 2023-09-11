using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.PubnubEventEngine
{
    public class HandshakeFailedEffectHandler : IEffectInvocationHandler
	{
		EventEmitter emitter;
		//public EffectInvocationType InvocationType { get; set; }
		private ExtendedState extendedState { get; set;}
		private PNStatus pnStatus { get; set; }
		public Action<string> LogCallback { get; set; }
		public Action<PNStatus> AnnounceStatus { get; set; }

		CancellationTokenSource? cancellationTokenSource;

		public HandshakeFailedEffectHandler(EventEmitter emitter)
		{
			this.emitter = emitter;
			cancellationTokenSource = new CancellationTokenSource();
		}

		public async void Start(ExtendedState context, EventType eventType)
		{
			extendedState = context;
			await Task.Factory.StartNew(() => {	});
			if (cancellationTokenSource != null && cancellationTokenSource.Token.CanBeCanceled) {
				Cancel();
			}

			if (eventType != EventType.HandshakeReconnectGiveUp)
			{
				LogCallback?.Invoke("HandshakeFailedEffectHandler - EventType.Handshake");
				Reconnect reconnectEvent = new Reconnect();
				reconnectEvent.Name = "RECONNECT";
				reconnectEvent.EventType = EventType.Reconnect;
			
				emitter.emit(reconnectEvent);
			}

		}

		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				LogCallback?.Invoke($"HandshakeFailedEffectHandler - cancellationTokenSource - cancellion attempted.");
				cancellationTokenSource.Cancel();
			}
		}
        public void Run(ExtendedState context)
        {
            if (AnnounceStatus != null)
			{
				AnnounceStatus(pnStatus);
			}
        }

        public PNStatus GetPNStatus()
        {
            return pnStatus;
        }
    }
}
