using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNEventEngine
{
	public class Event
	{
		public EventType Type { get; set; }
		public EventPayload EventPayload { get; set; }

		public Event()
		{
			EventPayload = new EventPayload();
		}
	}

	public class EventPayload
	{
		public List<string>? Channels { get; set; }
		public List<string>? ChannelGroups { get; set; }
		public string? Timetoken { get; set; }
		public int? Region { get; set; }

		public Exception? exception { get; set; }
	}


	public enum EventType
	{
		SubscriptionChange,
		HandshakeSuccess,
		ReceiveSuccess,
		HandshakeFailed,
		ReceiveFailed,
		ReconnectionFailed
	}

	public class ExtendedState
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public string Timetoken { get; set; }
		public int? Region { get; set; }
		public int AttemptedReconnection { get; set; }

		public ExtendedState()
		{
			Channels = new List<string>();
			ChannelGroups = new List<string>();
			Timetoken = "0";
			Region = 0;
			AttemptedReconnection = 0;
		}

	}

	public class EventEngine
	{
		public ExtendedState context;
		public State? CurrentState { get; set; }
		public List<State> States { get; set; }

		public EffectDispatcher dispatcher;

		public EventEmitter emitter;

		public EventEngine(EffectDispatcher dispatcher, EventEmitter emitter)
		{
			this.dispatcher = dispatcher;
			States = new List<State>();
			context = new ExtendedState();
			this.emitter = emitter;
			emitter.RegisterHandler(this.Transition);
		}

		public State CreateState(StateType type)
		{
			var newState = new State(type);
			States.Add(newState);
			return newState;
		}

		public void Transition(Event e)
		{
			StateType nextStateType;
			if (CurrentState != null && CurrentState.transitions.TryGetValue(e.Type, out nextStateType)) {
				CurrentState.Exit();
				CurrentState = this.States.Find((s) => s.Type == nextStateType);
				UpdateContext(e.EventPayload);
				if (CurrentState != null)
				{
					CurrentState.Entry();
					UpdateContext(e.EventPayload);
					if (CurrentState.Effects.Count > 0) {
						foreach (var effect in CurrentState.Effects) {
							System.Diagnostics.Debug.WriteLine("Found effect "+ effect);
							dispatcher.dispatch(effect, this.context);
						}
					}
				}
			}
		}

		public void Subscribe(List<string> channels, List<string>? channelGroups)
		{
			var evnt = new Event();
			evnt.Type = EventType.SubscriptionChange;
			evnt.EventPayload.Channels = channels;
			if (channelGroups != null) evnt.EventPayload.ChannelGroups = channelGroups;
			this.Transition(evnt);
		}

		private void UpdateContext(EventPayload eventData)
		{
			if (eventData.Channels != null) context.Channels = eventData.Channels;
			if (eventData.ChannelGroups != null) context.ChannelGroups = eventData.ChannelGroups;
			if (eventData.Timetoken != null) context.Timetoken = eventData.Timetoken;
			if (eventData.Region != null) context.Region = eventData.Region;
		}

		public void InitialState(State state)
		{
			this.CurrentState = state;
		}
	}
}
