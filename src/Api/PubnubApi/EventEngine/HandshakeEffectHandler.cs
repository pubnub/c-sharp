using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace PubnubApi.PubnubEventEngine
{
	public class HandshakeResponse
	{
		[JsonProperty("t")]
		public Timetoken? Timetoken { get; set; }

		[JsonProperty("m")]
		public object[]? Messages { get; set; }
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

	public class HandshakeEffectHandler : IEffectHandler
	{
		EventEmitter emitter;
		public Action<string> LogCallback { get; set; }

		public event EventHandler<HandshakeRequestEventArgs>? HandshakeRequested;
		protected virtual void OnHandshakeRequested(HandshakeRequestEventArgs e)
        {
            EventHandler<HandshakeRequestEventArgs>? handler = HandshakeRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

		CancellationTokenSource? cancellationTokenSource;
        public HandshakeEffectHandler(EventEmitter emitter)
		{
			this.emitter = emitter;
			cancellationTokenSource = new CancellationTokenSource();
		}
		public async void Start(ExtendedState context)
		{
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
			var evnt = new Event();
			try {
				LogCallback?.Invoke($"Handshake Json Response { json }");    
				var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(json);
				if (handshakeResponse != null)
				{
					evnt.EventPayload.Timetoken = handshakeResponse.Timetoken?.Timestamp;
					evnt.EventPayload.Region = handshakeResponse.Timetoken?.Region;
					evnt.Type = EventType.HandshakeSuccess;
					LogCallback?.Invoke("OnHandshakeEffectResponseReceived - EventType.HandshakeSuccess");  
				}
				else
				{
					evnt.Type = EventType.HandshakeFailed;
					LogCallback?.Invoke("OnHandshakeEffectResponseReceived - EventType.HandshakeFailed");  
				}
			} catch (Exception ex) {
				LogCallback?.Invoke($"OnHandshakeEffectResponseReceived EXCEPTION - {ex}");
				evnt.Type = EventType.HandshakeFailed;
				evnt.EventPayload.exception = ex;
			}
			emitter.emit(evnt);
			emitter.emit(json, true, 0);
		}
		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			LogCallback?.Invoke($"HandshakeEffectHandler - Handshake cancellion attempted.");
		}

    }
}
