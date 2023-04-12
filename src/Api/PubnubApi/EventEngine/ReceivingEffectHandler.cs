using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PubnubApi.PubnubEventEngine
{
	public class ReceiveingResponse
	{
		[JsonProperty("t")]
		public Timetoken? Timetoken { get; set; }

		[JsonProperty("m")]
		public Message[]? Messages { get; set; }
	}

	public class Message
	{
		[JsonProperty("c")]
		public string Channel { get; set; }

		[JsonProperty("d")]
		public object? Payload { get; set; }
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
	public class ReceivingEffectHandler<T> : IEffectHandler
	{
		EventEmitter emitter;
		public Action<string> LogCallback { get; set; }
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
			var evnt = new Event();
			int messageCount = 0;
			try {
				var receivedResponse = JsonConvert.DeserializeObject<ReceiveingResponse>(json);
				if (receivedResponse != null)
				{
					evnt.EventPayload.Timetoken = receivedResponse.Timetoken.Timestamp;
					evnt.EventPayload.Region = receivedResponse.Timetoken.Region;
					evnt.Type = EventType.ReceiveSuccess;

					if (receivedResponse.Messages != null && receivedResponse.Messages.Length > 0)
					{
						messageCount = receivedResponse.Messages.Length;
						for(int index = 0; index < receivedResponse.Messages.Length; index++)
						{
							LogCallback?.Invoke($"Received Message ({index+1} of {receivedResponse.Messages.Length}) : {JsonConvert.SerializeObject(receivedResponse.Messages[index])}");
							if (receivedResponse.Messages[index].Channel.IndexOf("-pnpres") > 0)
							{
								var presenceData = JsonConvert.DeserializeObject<PresenceEvent>(receivedResponse.Messages[index].Payload.ToString());
							}
							else
							{
								LogCallback?.Invoke($"Message : {JsonConvert.SerializeObject(receivedResponse.Messages[index].Payload)}");
							}
						}
					}
				}
			} catch (Exception ex) {
				LogCallback?.Invoke($"ReceivingEffectHandler EXCEPTION - {ex}");

				evnt.Type = EventType.ReceiveFailed;
				evnt.EventPayload.exception = ex;
			}
			emitter.emit(evnt);
			emitter.emit(json, false, messageCount);
		}

		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			LogCallback?.Invoke($"ReceivingEffectHandler - Receiving request cancellion attempted.");
		}
	}
}
