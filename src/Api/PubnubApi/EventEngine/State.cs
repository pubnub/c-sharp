using System;
using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine
{

	public enum StateType { Unsubscribed, Handshaking, Receiving, ReceiveReconnecting, ReceiveStopped, ReceiveFailed, HandshakeFailed, ReconnectingFailed, HandshakeReconnecting, HandshakeStopped };

	public class State
	{
		public EventType EventType {  get; set; }
		public StateType StateType { get; set; }
		public Dictionary<EventType, StateType> transitions;
		public List<EffectType> Effects { get; set; }
		public Func<bool> Entry { get; set; } = () => {
			return true;
		};

		public Func<bool> Exit { get; set; } = () => {
			return true;
		};

		public State(StateType type)
		{
			this.StateType = type;
			this.transitions = new Dictionary<EventType, StateType>();
			Effects = new List<EffectType>();
		}

		public State On(EventType e, StateType nextState)
		{
			transitions.Add(e, nextState);
			return this;
		}

		public State OnEntry(Func<bool> entry)
		{
			this.Entry = entry;
			return this;
		}

		public State OnExit(Func<bool> exit)
		{
			this.Exit = exit;
			return this;
		}

		public State Effect(EffectType effect)
		{
			this.Effects.Add(effect);
			return this;
		}
	}
}
