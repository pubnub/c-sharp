using System;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace PNEventEngine
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
		public string? Timestamp { get; set; }

		[JsonProperty("r")]
		public int? Region { get; set; }

	}

	public class HandshakeEffectHandler<T> : IEffectHandler
	{
		EventEmitter emitter;
		//HttpClient httpClient;

		//PNConfiguration pnConfig;
        public HandshakeEffectHandler(Action<T> httpRequestHandler, EventEmitter emitter)
		{
			this.emitter = emitter;
			//httpClient = client;
			//pnConfig = config;
		}
		public async void Start(ExtendedState context)
		{
			var evnt = new Event();
			await Task.Factory.StartNew(() => { });
			// TODO: Replace with Stateless Utility Methods
			// TODO: Fetch Configuration from PubNub instance
			try {
				//var res = await httpClient.GetAsync($"https://ps.pndsn.com/v2/subscribe/sub-c-c9710928-1b7a-11e3-a0c8-02ee2ddab7fe/{String.Join(",", context.Channels.ToArray())}/0?uuid=cSharpTest&tt=0&tr=0&channel-group={String.Join(", ", context.ChannelGroups.ToArray())}");
				//var jsonResp = await res.Content.ReadAsStringAsync();
				////LoggingMethod.WriteToLog(string.Format("HandshakeEffectHandler - {0}",jsonResp));
				//var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(jsonResp);
				//evnt.EventPayload.Timetoken = handshakeResponse.Timetoken.Timestamp;
				//evnt.EventPayload.Region = handshakeResponse.Timetoken.Region;
				//evnt.Type = EventType.HandshakeSuccess;
			} catch (Exception ex) {
				//LoggingMethod.WriteToLog(string.Format("HandshakeEffectHandler EXCEPTION - {0}",ex));
				evnt.Type = EventType.HandshakeFailed;
				evnt.EventPayload.exception = ex;
			}
			emitter.emit(evnt);
		}
		public void Cancel()
		{
			//Console.WriteLine("Handshake can not be cancelled. Something is not right here!");
			//LoggingMethod.WriteToLog("HandshakeEffectHandler - Handshake can not be cancelled. Something is not right here!");
		}

    }
}
