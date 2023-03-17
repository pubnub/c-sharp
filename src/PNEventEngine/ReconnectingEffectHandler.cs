using System;

namespace PNEventEngine
{
	public class ReconnectingEffectHandler<T> : IEffectHandler
	{
		EventEmitter eventEmitter;

		//PNConfiguration pnConfig;
        public ReconnectingEffectHandler(Action<T> httpRequestHandler, EventEmitter emitter)
		{
			this.eventEmitter = emitter;
			//pnConfig = config;
		}

		public void Start(ExtendedState context)    // TODO: Implementation of retry  getDelay() as per policy
		{
			var evnt = new Event();
			evnt.EventPayload.Timetoken = context.Timetoken;
			evnt.EventPayload.Region = context.Region;
			evnt.Type = EventType.ReceiveSuccess;
			this.eventEmitter.emit(evnt);
		}

		public void Cancel()
		{
			System.Diagnostics.Debug.WriteLine("Reconnecting Cancelled!!!");
		}
	}
}
