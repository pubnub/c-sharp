using System;
using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine
{

	public enum StateType { Unsubscribed, Handshaking, HandshakingFailed, Receiving, ReceiveReconnecting, ReceiveStopped, ReceiveFailed, HandshakeFailed, ReconnectingFailed, HandshakeReconnecting, HandshakeStopped };

	public class State
	{
		public EventType EventType {  get; set; }
		public StateType StateType { get; set; }

		public Dictionary<EventType, StateType> transitions;
		public List<EffectInvocationType> EffectInvocationsList { get; private set; }
		public List<IEffectInvocationHandler> EntryList { get; private set; }

		public List<IEffectInvocationHandler> ExitList { get; private set; }

		public State(StateType type)
		{
			this.StateType = type;
			this.transitions = new Dictionary<EventType, StateType>();
			EffectInvocationsList = new List<EffectInvocationType>();
		}

		public State On(EventType e, StateType nextState)
		{
			transitions.Add(e, nextState);
			return this;
		}

		public State OnEntry(List<IEffectInvocationHandler> entryInvocationList)
		{
			this.EntryList = entryInvocationList;
			return this;
		}

		public State OnExit(List<IEffectInvocationHandler> exitInvocationList)
		{
			this.ExitList = exitInvocationList;
			return this;
		}

		public State EffectInvocation(EffectInvocationType trigger, IEffectInvocationHandler effectInvocationHandler)
		{
			this.EffectInvocationsList.Add(trigger);
			return this;
		}
	}
}
