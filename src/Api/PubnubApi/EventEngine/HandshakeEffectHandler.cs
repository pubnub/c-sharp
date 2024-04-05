using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace PubnubApi.PubnubEventEngine
{
	public class HandshakeResponse
	{
		[JsonProperty("t")]
		public Timetoken Timetoken { get; set; }

		[JsonProperty("m")]
		public object[] Messages { get; set; }
	}
	public class HandshakeError
	{
		[JsonProperty("status")]
		public int Status { get; set; }

		[JsonProperty("error")]
		public string ErrorMessage { get; set; }
	}

	public class Timetoken
	{
		[JsonProperty("t")]
		public long? Timestamp { get; set; }

		[JsonProperty("r")]
		public int? Region { get; set; }

	}

	public class HandshakeRequestEventArgs : EventArgs 
	{
		public ExtendedState ExtendedState { get; set; }
		public Action<string> HandshakeResponseCallback { get; set; }
	}
	public class CancelHandshakeRequestEventArgs : EventArgs 
	{
	}

	public class HandshakeEffectHandler : IEffectInvocationHandler
	{
		EventEmitter emitter;
		private ExtendedState extendedState { get; set;}
		public Action<string> LogCallback { get; set; }
		public Action<PNStatus> AnnounceStatus { get; set; }
		private PNStatus pnStatus { get; set; }

		public event EventHandler<HandshakeRequestEventArgs> HandshakeRequested;
		public event EventHandler<CancelHandshakeRequestEventArgs> CancelHandshakeRequested;
		protected virtual void OnHandshakeRequested(HandshakeRequestEventArgs e)
        {
            EventHandler<HandshakeRequestEventArgs> handler = HandshakeRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }
		protected virtual void OnCancelHandshakeRequested(CancelHandshakeRequestEventArgs e)
        {
            EventHandler<CancelHandshakeRequestEventArgs> handler = CancelHandshakeRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

		CancellationTokenSource cancellationTokenSource;
        public HandshakeEffectHandler(EventEmitter emitter)
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
			HandshakeRequestEventArgs args = new HandshakeRequestEventArgs();
			args.ExtendedState = context;
			args.HandshakeResponseCallback = OnHandshakeEffectResponseReceived;
			OnHandshakeRequested(args);				
		}
		
		public void OnHandshakeEffectResponseReceived(string json)
		{
			try
			{
				LogCallback?.Invoke($"OnHandshakeEffectResponseReceived Json Response = {json}");
				var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(json);
				if (handshakeResponse != null && handshakeResponse.Timetoken != null)
				{
					HandshakeSuccess handshakeSuccessEvent = new HandshakeSuccess();
					handshakeSuccessEvent.SubscriptionCursor = new SubscriptionCursor();
					handshakeSuccessEvent.SubscriptionCursor.Timetoken = handshakeResponse.Timetoken?.Timestamp;
					handshakeSuccessEvent.SubscriptionCursor.Region = handshakeResponse.Timetoken?.Region;

					handshakeSuccessEvent.EventPayload.Timetoken = handshakeResponse.Timetoken?.Timestamp;
					handshakeSuccessEvent.EventPayload.Region = handshakeResponse.Timetoken?.Region;
					handshakeSuccessEvent.EventType = EventType.HandshakeSuccess;
					handshakeSuccessEvent.Name = "HANDSHAKE_SUCCESS";
					LogCallback?.Invoke("OnHandshakeEffectResponseReceived - EventType.HandshakeSuccess");
					
					pnStatus = new PNStatus();
					pnStatus.StatusCode = 200;
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Category = PNStatusCategory.PNConnectedCategory;
					pnStatus.Error = false;
					
					emitter.emit(handshakeSuccessEvent);
				}
				else
				{
					HandshakeFailure handshakeFailureEvent = new HandshakeFailure();
					handshakeFailureEvent.Name = "HANDSHAKE_FAILURE";
					handshakeFailureEvent.EventType = EventType.HandshakeFailure;
					LogCallback?.Invoke("OnHandshakeEffectResponseReceived - EventType.HandshakeFailure");

					pnStatus = new PNStatus();
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Category = PNStatusCategory.PNNetworkIssuesCategory;
					pnStatus.Error = true;

					emitter.emit(handshakeFailureEvent);
				}
			}
			catch (Exception ex)
			{
				LogCallback?.Invoke($"OnHandshakeEffectResponseReceived EXCEPTION - {ex}");
				HandshakeFailure handshakeFailureEvent = new HandshakeFailure();
				handshakeFailureEvent.Name = "HANDSHAKE_FAILURE";
				handshakeFailureEvent.EventType = EventType.HandshakeFailure;
				handshakeFailureEvent.EventPayload.exception = ex;

				pnStatus = new PNStatus();
				pnStatus.Operation = PNOperationType.PNSubscribeOperation;
				pnStatus.AffectedChannels = extendedState.Channels;
				pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
				pnStatus.Category = PNStatusCategory.PNNetworkIssuesCategory;
				pnStatus.Error = true;

				emitter.emit(handshakeFailureEvent);
			}
			
			//emitter.emit(json, true, 0);
		}
		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				LogCallback?.Invoke($"HandshakeEffectHandler - cancellationTokenSource - cancellion attempted.");
				cancellationTokenSource.Cancel();
			}

			LogCallback?.Invoke($"HandshakeEffectHandler - invoking OnCancelHandshakeRequested.");
			CancelHandshakeRequestEventArgs args = new CancelHandshakeRequestEventArgs();
			OnCancelHandshakeRequested(args);				
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
