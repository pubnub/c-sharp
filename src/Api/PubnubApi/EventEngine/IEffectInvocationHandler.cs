using System;

namespace PubnubApi.PubnubEventEngine
{

	public enum EffectInvocationType
	{
		Handshake,
		HandshakeSuccess,  
		CancelHandshake,
		ReceiveMessages,
		ReceiveSuccess,
		Disconnect,
		HandshakeReconnect,
		HandshakeReconnectSuccess,
		CancelHandshakeReconnect,
		CancelReceiveMessages,
		ReceiveReconnect,
		ReceiveReconnectSuccess,
		ReceiveReconnectGiveUp,
		CancelReceiveReconnect,
		ReconnectionAttempt
	}

	public interface IEffectInvocationHandler
	{
		void Start(ExtendedState context, EventType eventType);
		void Cancel();
		void Run(ExtendedState context);
		PNStatus GetPNStatus();
	}

	public interface IReceiveMessageHandler<T>
	{
		Message<object>[] GetMessages();
	}
}
