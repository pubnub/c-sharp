using System;

namespace PNEventEngine
{

	public enum EffectType
	{
		SendHandshakeRequest,  // oneshot
		ReceiveEventRequest,   // long running
		ReconnectionAttempt
	}

	public interface IEffectHandler
	{
		void Start(ExtendedState context);
		void Cancel();
	}
}
