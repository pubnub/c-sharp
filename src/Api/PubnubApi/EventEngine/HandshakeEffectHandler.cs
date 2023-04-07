using System;
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

        public HandshakeEffectHandler(EventEmitter emitter)
		{
			this.emitter = emitter;
		}
		public async void Start(ExtendedState context)
		{
			await Task.Factory.StartNew(() => {	});
			HandshakeRequestEventArgs args = new HandshakeRequestEventArgs();
			args.ExtendedState = context;
			args.HandshakeResponseCallback = OnHandshakeEffectResponseReceived;
			OnHandshakeRequested(args);				
			// TODO: Replace with Stateless Utility Methods
			// TODO: Fetch Configuration from PubNub instance
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
			} catch (Exception ex) {
				LogCallback?.Invoke($"OnHandshakeEffectResponseReceived EXCEPTION - {ex}");
				evnt.Type = EventType.HandshakeFailed;
				evnt.EventPayload.exception = ex;
			}
			emitter.emit(evnt);
			emitter.emit(json, true);
		}
		public void Cancel()
		{
			//Console.WriteLine("Handshake can not be cancelled. Something is not right here!");
			//LoggingMethod.WriteToLog("HandshakeEffectHandler - Handshake can not be cancelled. Something is not right here!");
			LogCallback?.Invoke($"HandshakeEffectHandler - Handshake can not be cancelled. Something is not right here!");
		}

    }
}
