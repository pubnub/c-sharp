using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PubnubApi.PubnubEventEngine
{
	public class ReceiveingResponse<T>
	{
		[JsonProperty("t")]
		public Timetoken Timetoken { get; set; }

		[JsonProperty("m")]
		public Message<T>[] Messages { get; set; }
	}

	public class Message<T>
	{
		[JsonProperty ("a")]
		public string Shard { get; set;}

		[JsonProperty ("b")]
		public string SubscriptionMatch { get; set;}

		[JsonProperty("c")]
		public string Channel { get; set; }

		[JsonProperty("d")]
		public T Payload { get; set; }

		[JsonProperty("e")]
		public int MessageType { get; set; }

		[JsonProperty("f")]
		public string Flags { get; set; }

		//[JsonProperty("i")]
		//public string IssuingClientId { get; set; }

		[JsonProperty("k")]
		public string SubscribeKey { get; set; }

		[JsonProperty("o")]
		public object OriginatingTimetoken { get; set; }

		[JsonProperty("p")]
		public object PublishMetadata { get; set; }

		[JsonProperty("s")]
		public long SequenceNumber { get; set; }
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

	public class ReceiveError
	{
		[JsonProperty("status")]
		public int Status { get; set; }

		[JsonProperty("error")]
		public string ErrorMessage { get; set; }
	}

	public class ReceiveRequestEventArgs : EventArgs 
	{
		public ExtendedState ExtendedState { get; set; }
		public Action<string> ReceiveResponseCallback { get; set; }
	}

	public class CancelReceiveRequestEventArgs : EventArgs
	{

	}

	public class ReceivingEffectHandler<T> : IEffectInvocationHandler, IReceiveMessageHandler<T>
	{
		EventEmitter emitter;
		private ExtendedState extendedState { get; set;}
		private PNStatus pnStatus { get; set; }
		private Message<object>[] receiveMessages { get; set; }
		public Action<string> LogCallback { get; set; }
		public Action<PNStatus> AnnounceStatus { get; set; }
		public Action<PNMessageResult<object>> AnnounceMessage { get; set; }
		public Action<PNPresenceEventResult> AnnouncePresenceEvent { get; set; }
		public PNReconnectionPolicy ReconnectionPolicy { get; set; }

		public event EventHandler<ReceiveRequestEventArgs> ReceiveRequested;
		public event EventHandler<CancelReceiveRequestEventArgs> CancelReceiveRequested;
		protected virtual void OnReceiveRequested(ReceiveRequestEventArgs e)
        {
            EventHandler<ReceiveRequestEventArgs> handler = ReceiveRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

		protected virtual void OnCancelReceiveRequested(CancelReceiveRequestEventArgs e)
        {
            EventHandler<CancelReceiveRequestEventArgs> handler = CancelReceiveRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

		CancellationTokenSource cancellationTokenSource;

        public ReceivingEffectHandler(EventEmitter emitter)
		{
			this.emitter = emitter;
			cancellationTokenSource = new CancellationTokenSource();
		}

		public async void Start(ExtendedState context, EventType eventType)
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
				pnStatus = null;
				var receivedResponse = JsonConvert.DeserializeObject<ReceiveingResponse<object>>(json);
				if (receivedResponse != null && receivedResponse.Timetoken != null)
				{
					receiveMessages = receivedResponse.Messages;

					ReceiveSuccess receiveSuccessEvent = new ReceiveSuccess();
					receiveSuccessEvent.SubscriptionCursor = new SubscriptionCursor();
					receiveSuccessEvent.SubscriptionCursor.Timetoken = receivedResponse.Timetoken.Timestamp;
					receiveSuccessEvent.SubscriptionCursor.Region = receivedResponse.Timetoken.Region;
					receiveSuccessEvent.EventPayload.Timetoken = receivedResponse.Timetoken.Timestamp;
					receiveSuccessEvent.EventPayload.Region = receivedResponse.Timetoken.Region;
					receiveSuccessEvent.EventType = EventType.ReceiveSuccess;
					receiveSuccessEvent.Name = "RECEIVE_SUCCESS";
					LogCallback?.Invoke("OnReceivingEffectResponseReceived - EventType.ReceiveSuccess");

					emitter.emit(receiveSuccessEvent);
				}
				else
				{
					ReceiveFailure receiveFailureEvent = new ReceiveFailure();
					receiveFailureEvent.Name = "RECEIVE_FAILURE";
					receiveFailureEvent.EventType = EventType.ReceiveFailure;
					LogCallback?.Invoke("OnReceivingEffectResponseReceived - EventType.ReceiveFailure");

					pnStatus = new PNStatus();
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
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
				receiveFailureEvent.Name = "RECEIVE_FAILURE";
				receiveFailureEvent.EventType = EventType.ReceiveFailure;
				receiveFailureEvent.EventPayload.exception = ex;
				LogCallback?.Invoke("OnReceivingEffectResponseReceived - EventType.ReceiveFailure");

				pnStatus = new PNStatus();
				pnStatus.Operation = PNOperationType.PNSubscribeOperation;
				pnStatus.AffectedChannels = extendedState.Channels;
				pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
				pnStatus.Error = true;

				emitter.emit(receiveFailureEvent);
			}
		}

		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				LogCallback?.Invoke($"ReceivingEffectHandler - Receiving request cancellion attempted.");
				cancellationTokenSource.Cancel();
			}
			LogCallback?.Invoke($"ReceivingEffectHandler - invoking OnCancelReceiveRequested.");
			CancelReceiveRequestEventArgs args = new CancelReceiveRequestEventArgs();
			OnCancelReceiveRequested(args);				
		}
        public void Run(ExtendedState context)
        {
            if (AnnounceStatus != null && pnStatus != null)
			{
				AnnounceStatus(pnStatus);
			}
			Message<object>[] receiveMessages = GetMessages();
			int messageCount = (receiveMessages != null) ? receiveMessages.Length : 0;
			if (messageCount > 0)
			{
				for (int index = 0; index < receiveMessages.Length; index++)
				{
					LogCallback?.Invoke($"Received Message ({index + 1} of {receiveMessages.Length}) : {JsonConvert.SerializeObject(receiveMessages[index])}");
					if (receiveMessages[index].Channel.IndexOf("-pnpres") > 0)
					{
						if (AnnouncePresenceEvent != null)
						{
							var presenceEvent = JsonConvert.DeserializeObject<PresenceEvent>(receiveMessages[index].Payload.ToString());
							PNPresenceEventResult presenceEventResult = new PNPresenceEventResult();
							presenceEventResult.Channel = receiveMessages[index].Channel;
							presenceEventResult.Event = presenceEvent.Action;
							presenceEventResult.Occupancy = presenceEvent.Occupancy;
							presenceEventResult.Uuid = presenceEvent.Uuid;
							presenceEventResult.Timestamp = presenceEvent.Timestamp;
							presenceEventResult.UserMetadata = receiveMessages[index].PublishMetadata;

							AnnouncePresenceEvent?.Invoke(presenceEventResult);
						}
					}
					else
					{
						if (receiveMessages[index].MessageType == 1)
						{
							//TODO: Callback for Signal message
                            PNSignalResult<object> signalMessage = new PNSignalResult<object>
                            {
                                Channel = receiveMessages[index].Channel,
                                Message = receiveMessages[index].Payload,
                            };
							AnnounceMessage?.Invoke(signalMessage);
						}
						else if (receiveMessages[index].MessageType == 2)
						{
							//TODO: Callback for Object message
						}
						else if (receiveMessages[index].MessageType == 3)
						{
							//TODO: Callback for Message Action message
						}
						else if (receiveMessages[index].MessageType == 4)
						{
							//TODO: Callback for File message
						}
						else
						{
							//Callback for regular message
							if (AnnounceMessage != null)
							{
								LogCallback?.Invoke($"Message : {JsonConvert.SerializeObject(receiveMessages[index].Payload)}");
								PNMessageResult<object> messageResult = new PNMessageResult<object>();
								messageResult.Channel = receiveMessages[index].Channel;
								messageResult.Message = receiveMessages[index].Payload;
								AnnounceMessage?.Invoke(messageResult);
							}
						}
					}
				}
			}
        }

        public PNStatus GetPNStatus()
        {
            return pnStatus;
        }

        public Message<object>[] GetMessages()
        {
			return receiveMessages;
        }
    }
}
