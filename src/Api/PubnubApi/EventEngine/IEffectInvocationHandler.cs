using System;

namespace PubnubApi.PubnubEventEngine
{

	public enum EffectInvocationType
	{
		HandshakeSuccess,  
		ReceiveSuccess,
		Disconnect,
		CancelHandshake,
		HandshakeReconnect,
		CancelHandshakeReconnect,
		CancelReceiveMessages,
		ReceiveReconnect,
		CancelReceiveReconnect,
		ReconnectionAttempt
	}

	public interface IEffectInvocationHandler
	{
		void Start(ExtendedState context);
		void Cancel();
	}
}
