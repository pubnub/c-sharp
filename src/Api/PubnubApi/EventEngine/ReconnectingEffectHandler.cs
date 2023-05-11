using System;

namespace PubnubApi.PubnubEventEngine
{
	public class ReconnectingEffectHandler<T> : IEffectInvocationHandler
	{
		EventEmitter eventEmitter;
		private PNStatus pnStatus { get; set; }
        public ReconnectingEffectHandler(EventEmitter emitter)
		{
			this.eventEmitter = emitter;
		}

		public void Start(ExtendedState context, EventType eventType)    // TODO: Implementation of retry  getDelay() as per policy
		{
			var evnt = new Reconnect();
			evnt.EventPayload.Timetoken = context.Timetoken;
			evnt.EventPayload.Region = context.Region;
			evnt.EventType = EventType.ReceiveSuccess;
			evnt.Name = "RECEIVE_RECONNECT_SUCCESS";
			this.eventEmitter.emit(evnt);
		}

		public void Cancel()
		{
			System.Diagnostics.Debug.WriteLine("ReconnectingEffectHandler Cancelled!!!");
		}
		public PNStatus GetPNStatus()
        {
            return pnStatus;
        }
	}
}
