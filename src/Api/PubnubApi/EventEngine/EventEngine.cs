using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class Event
	{
		public EventType EventType { get; set; }
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

	public class EffectInvocation
	{
		public EventType Effectype { get; set; }
		public IEffectInvocationHandler Handler { get; set; }
	}
	public enum EventType
	{
		SubscriptionChanged,
		SubscriptionRestored,
		Handshake,
		CancelHandshake,
		HandshakeSuccess,
		ReceiveMessages,
		CancelReceiveMessages,
		ReceiveSuccess,
		HandshakeFailure,
		ReceiveFailure,
		ReceiveReconnect,
		CancelReceiveReconnect,
		ReceiveReconnectFailure,
		ReceiveReconnectSuccess,
		ReceiveReconnectGiveUp,
		HandshakeReconnect,
		CancelHandshakeReconnect,
		HandshakeReconnectSuccess,
		HandshakeReconnectFailure,
		HandshakeReconnectGiveUp,
		HandshakeReconnectRetry,
		ReconnectionFailed,
		Disconnect,
		Reconnect
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
		public IPubnubUnitTest PubnubUnitTest { get; set; }

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
			if (CurrentState != null && CurrentState.transitions.TryGetValue(e.EventType, out nextStateType)) {
				System.Diagnostics.Debug.WriteLine($"Current State = {CurrentState.StateType}; Transition = {e.EventType}");
				if (CurrentState != null && CurrentState.ExitList != null && CurrentState.ExitList.Count > 0)
				{
					foreach(var entry in CurrentState.ExitList)
					{
						entry.Handler?.Cancel();
					}
				}
				CurrentState = this.States.Find((s) => s.StateType == nextStateType);
				UpdateContext(e.EventType, e.EventPayload);
				if (CurrentState != null)
				{
					if (CurrentState.EntryList != null && CurrentState.EntryList.Count > 0)
					{
						foreach(var entry in CurrentState.EntryList)
						{
							entry.Handler?.Start(Context);
						}
					}
					System.Diagnostics.Debug.WriteLine($"Transition = {e.EventType}");
					UpdateContext(e.EventType, e.EventPayload);
					if (CurrentState.EffectInvocationsList.Count > 0) {
						foreach (var effect in CurrentState.EffectInvocationsList) {
							if (e.EventType == effect.Effectype)
							{
								System.Diagnostics.Debug.WriteLine("Found effect "+ effect.Effectype);
								Dispatcher.dispatch(effect.Effectype, this.Context);
							}
						}
					}
				}
			}
		}

		public void Subscribe(List<string> channels, List<string>? channelGroups)
		{
			var evnt = new Event();
			evnt.EventType = EventType.SubscriptionChanged;
			evnt.EventPayload.Channels = channels;
			if (channelGroups != null) evnt.EventPayload.ChannelGroups = channelGroups;
			this.Transition(evnt);
		}

		private void UpdateContext(EventType eventType, EventPayload eventData)
		{
			CurrentState.EventType = eventType;
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
