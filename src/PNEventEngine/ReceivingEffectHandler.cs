using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PNEventEngine
{
	public class ReceiveingResponse
	{
		[JsonProperty("t")]
		public Timetoken? Timetoken { get; set; }

		[JsonProperty("m")]
		public object[]? Messages { get; set; }
	}

	public class ReceivingEffectHandler<T> : IEffectHandler
	{
		EventEmitter emitter;
		//HttpClient httpClient;
		CancellationTokenSource? cancellationTokenSource;

		//PNConfiguration pnConfig;
        public ReceivingEffectHandler(Action<T> httpRequestHandler, EventEmitter emitter)
		{
			this.emitter = emitter;
			//httpClient = client;
			//pnConfig = config;
			cancellationTokenSource = new CancellationTokenSource();
		}

		public async void Start(ExtendedState context)
		{
			await Task.Factory.StartNew(() => { });
			if (cancellationTokenSource != null && cancellationTokenSource.Token.CanBeCanceled) {
				Cancel();
			}
			cancellationTokenSource = new CancellationTokenSource();
			var evnt = new Event();
			// TODO: Replace with stateless Utility method...
			try {
				//var res = await httpClient.GetAsync($"https://ps.pndsn.com/v2/subscribe/sub-c-c9710928-1b7a-11e3-a0c8-02ee2ddab7fe/{String.Join(",", context.Channels.ToArray())}/0?uuid=cSharpTest&channel-group={String.Join(",", context.ChannelGroups.ToArray())}&tt={context.Timetoken}&tr={context.Region}", cancellationTokenSource.Token);
				//string jsonResp = await res.Content.ReadAsStringAsync();
				////LoggingMethod.WriteToLog(string.Format("ReceivingEffectHandler - {0}",jsonResp));
				//var receivedResponse = JsonConvert.DeserializeObject<ReceiveingResponse>(jsonResp);
				//evnt.EventPayload.Timetoken = receivedResponse.Timetoken.Timestamp;
				//evnt.EventPayload.Region = receivedResponse.Timetoken.Region;
				//evnt.Type = EventType.ReceiveSuccess;

				//if (receivedResponse.Messages != null)
				//	Console.WriteLine($"Received Messages {JsonConvert.SerializeObject(receivedResponse.Messages)}");    //WIP: Define "DELIVERING" Effect. and transition

			} catch (Exception ex) {
				evnt.Type = EventType.ReceiveFailed;
				//LoggingMethod.WriteToLog(string.Format("ReceivingEffectHandler EXCEPTION - {0}",ex));
				evnt.EventPayload.exception = ex;
			}
			emitter.emit(evnt);
		}
		public void Cancel()
		{
			//Console.WriteLine("Attempting cancellation");
			//LoggingMethod.WriteToLog("ReceivingEffectHandler - Attempting cancellation");
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
		}
	}
}
