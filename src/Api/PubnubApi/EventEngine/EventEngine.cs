using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
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
		public long? Timetoken { get; set; }
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
		public long? Timetoken { get; set; }
		public int? Region { get; set; }
		public int AttemptedReconnection { get; set; }

		public ExtendedState()
		{
			Channels = new List<string>();
			ChannelGroups = new List<string>();
			Timetoken = 0;
			Region = 0;
			AttemptedReconnection = 0;
		}

	}

	public class EventEngine
	{
		public ExtendedState Context;
		public State? CurrentState { get; set; }
		public List<State> States { get; set; }

		public EffectDispatcher Dispatcher;

		public EventEmitter Emitter;

		public EventEngine(EffectDispatcher dispatcher, EventEmitter emitter)
		{
			this.Dispatcher = dispatcher;
			States = new List<State>();
			Context = new ExtendedState();
			this.Emitter = emitter;
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
				System.Diagnostics.Debug.WriteLine($"Transition = {e.Type}");
				UpdateContext(e.Type, e.EventPayload);
				if (CurrentState != null)
				{
					CurrentState.Entry();
					System.Diagnostics.Debug.WriteLine($"Transition = {e.Type}");
					UpdateContext(e.Type, e.EventPayload);
					if (CurrentState.Effects.Count > 0) {
						foreach (var effect in CurrentState.Effects) {
							System.Diagnostics.Debug.WriteLine("Found effect "+ effect);
							Dispatcher.dispatch(effect, this.Context);
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

		private void UpdateContext(EventType eventType, EventPayload eventData)
		{
			if (eventData.Channels != null) Context.Channels = eventData.Channels;
			if (eventData.ChannelGroups != null) Context.ChannelGroups = eventData.ChannelGroups;
			if (eventData.Timetoken != null) 
			{
				System.Diagnostics.Debug.WriteLine($"eventData.Timetoken = {eventData.Timetoken.Value}");
				System.Diagnostics.Debug.WriteLine($"Context.Timetoken = {Context.Timetoken.Value}");
				if (Context.Timetoken > 0 && 
					eventType == EventType.HandshakeSuccess && 
					Context.Timetoken < eventData.Timetoken)
				{
					System.Diagnostics.Debug.WriteLine("Keeping last Context.Timetoken");
					// do not change context timetoken. We want last timetoken.
				}
				else
				{
					Context.Timetoken = eventData.Timetoken; 
				}
			}
			if (eventData.Region != null) Context.Region = eventData.Region;
		}

		public void InitialState(State state)
		{
			this.CurrentState = state;
		}
	}
}
