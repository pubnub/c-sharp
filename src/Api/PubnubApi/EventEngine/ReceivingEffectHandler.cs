using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PubnubApi.PubnubEventEngine
{
	public class ReceiveingResponse<T>
	{
		[JsonProperty("t")]
		public Timetoken? Timetoken { get; set; }

		[JsonProperty("m")]
		public Message<T>[] Messages { get; set; }
	}

	public class Message<T>
	{
		[JsonProperty("c")]
		public string Channel { get; set; }

		[JsonProperty("d")]
		public T Payload { get; set; }
	}
	public class PresenceEvent
	{
		[JsonProperty("action")]
		public string Action { get; set; }

		[JsonProperty("uuid")]
		public string Uuid { get; set; }

		[JsonProperty("timestamp")]
		public long Timestamp { get; set; }

		[JsonProperty("occupancy")]
        public int Occupancy { get; set; }

	}


	public class ReceiveRequestEventArgs : EventArgs 
	{
		public ExtendedState ExtendedState { get; set; }
		public Action<string> ReceiveResponseCallback { get; set; }
	}
	public class ReceivingEffectHandler<T> : IEffectInvocationHandler, IReceiveMessageHandler<T>
	{
		EventEmitter emitter;
		private ExtendedState extendedState { get; set;}
		private PNStatus pnStatus { get; set; }
		private Message<T>[] receiveMessages { get; set; }
		public Action<string> LogCallback { get; set; }
		//public Action<PNStatus> StatusCallback { get; set; }
		public event EventHandler<ReceiveRequestEventArgs>? ReceiveRequested;
		protected virtual void OnReceiveRequested(ReceiveRequestEventArgs e)
        {
            EventHandler<ReceiveRequestEventArgs>? handler = ReceiveRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

		CancellationTokenSource? cancellationTokenSource;

        public ReceivingEffectHandler(EventEmitter emitter)
		{
			this.emitter = emitter;
			cancellationTokenSource = new CancellationTokenSource();
		}

		public async void Start(ExtendedState context)
		{
			extendedState = context;
			await Task.Factory.StartNew(() => { });
			if (cancellationTokenSource != null && cancellationTokenSource.Token.CanBeCanceled) {
				Cancel();
			}
			cancellationTokenSource = new CancellationTokenSource();

			await Task.Factory.StartNew(() => {	});
			ReceiveRequestEventArgs args = new ReceiveRequestEventArgs();
			args.ExtendedState = context;
			args.ReceiveResponseCallback = OnReceivingEffectResponseReceived;
			OnReceiveRequested(args);				
		}

		public void OnReceivingEffectResponseReceived(string json)
		{
			try
			{
				var receivedResponse = JsonConvert.DeserializeObject<ReceiveingResponse<T>>(json);
				if (receivedResponse != null)
				{
					receiveMessages = receivedResponse.Messages;

					ReceiveSuccess receiveSuccessEvent = new ReceiveSuccess();
					receiveSuccessEvent.SubscriptionCursor = new SubscriptionCursor();
					receiveSuccessEvent.SubscriptionCursor.Timetoken = receivedResponse.Timetoken.Timestamp;
					receiveSuccessEvent.SubscriptionCursor.Region = receivedResponse.Timetoken.Region;
					receiveSuccessEvent.EventPayload.Timetoken = receivedResponse.Timetoken.Timestamp;
					receiveSuccessEvent.EventPayload.Region = receivedResponse.Timetoken.Region;
					receiveSuccessEvent.EventType = EventType.ReceiveSuccess;

					pnStatus = new PNStatus();
					pnStatus.StatusCode = 200;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Category = PNStatusCategory.PNConnectedCategory;
					pnStatus.Error = false;

					emitter.emit(receiveSuccessEvent);
				}
				else
				{
					ReceiveFailure receiveFailureEvent = new ReceiveFailure();
					receiveFailureEvent.EventType = EventType.ReceiveFailure;
					LogCallback?.Invoke("OnReceivingEffectResponseReceived - EventType.ReceiveFailure");

					pnStatus = new PNStatus();
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Error = true;

					emitter.emit(receiveFailureEvent);
				}
			}
			catch (Exception ex)
			{
				LogCallback?.Invoke($"ReceivingEffectHandler EXCEPTION - {ex}");

				ReceiveFailure receiveFailureEvent = new ReceiveFailure();
				receiveFailureEvent.EventType = EventType.ReceiveFailure;
				receiveFailureEvent.EventPayload.exception = ex;
				LogCallback?.Invoke("OnReceivingEffectResponseReceived - EventType.ReceiveFailure");

				pnStatus = new PNStatus();
				pnStatus.AffectedChannels = extendedState.Channels;
				pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
				pnStatus.Error = true;

				emitter.emit(receiveFailureEvent);
			}
			//emitter.emit(evnt);
			//emitter.emit(json, false, messageCount);
		}

		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			LogCallback?.Invoke($"ReceivingEffectHandler - Receiving request cancellion attempted.");
		}

        public PNStatus GetPNStatus()
        {
            return pnStatus;
        }

        public Message<T>[] GetMessages()
        {
			return receiveMessages;
        }
    }
}
