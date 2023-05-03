using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class PubnubError : Exception
	{

	}

    #region Event
    public abstract class Event
	{
		public virtual string Name { get; set; }
		public virtual EventType EventType { get; set; }
		public virtual EventPayload EventPayload { get; set; }
		public virtual int Attempts { get; set; }

		public Event()
		{
			EventPayload = new EventPayload();
		}
	}
    
	public class SubscriptionChanged : Event
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
	}
	public class Disconnect : Event
	{

	}
	public class Reconnect : Event
	{

	}
	public class HandshakeSuccess : Event
	{
		public SubscriptionCursor SubscriptionCursor { get; set; }
	}
	public class SubscriptionRestored : Event
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
	}
	public class HandshakeFailure : Event
	{
		public PubnubError Reason { get; set; }
	}
	public class HandshakeReconnectGiveUp : Event
	{
		public PubnubError Reason { get; set;}
	} 
	public class HandshakeReconnectSuccess : Event
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
	}
	public class HandshakeReconnectFailure : Event
	{
		public PubnubError Reason { get; set;}
	}
	public class HandshakeReconnectRetry : Event
	{

	}
	public class ReceiveSuccess : Event
	{
		public List<EventType> Messages { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
	}
	public class ReceiveFailure : Event
	{
		public PubnubError Reason { get; set;}
	}
	public class ReceiveReconnectFailure : Event
	{
		public PubnubError Reason { get; set;}
	}
	public class ReceiveReconnectGiveUp : Event
	{
		public PubnubError Reason { get; set;}
	}
	public class ReceiveReconnectSuccess : Event
	{
		public List<EventType> Messages { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
	}
	public class ReceiveReconnectRetry : Event
	{

	}
	public class Fail : Event
	{

	}
	public class Success : Event
	{

	}
	#endregion
    public class EventPayload
	{
		public List<string>? Channels { get; set; }
		public List<string>? ChannelGroups { get; set; }
		public long? Timetoken { get; set; }
		public int? Region { get; set; }

		public Exception? exception { get; set; }
	}


	public class SubscriptionCursor
	{
		public long? Timetoken { get; set; }
		public int? Region { get; set; }
	}

    #region EffectInvocation
	public abstract class EffectInvocation
	{
		public virtual string Name { get; set; }
		public virtual EventType Effectype { get; set; }
		public virtual IEffectInvocationHandler Handler { get; set; }
	}
    public class ReceiveMessages: EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
	}
	public class CancelReceiveMessages : EffectInvocation
	{

	}
	public class ReceiveReconnect: EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
		public int Attempts { get; set; }
		public PubnubError Reason { get; set; }
	}
	public class CancelReceiveReconnect : EffectInvocation
	{

	}
	public class Handshake : EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
	}
	public class CancelHandshake : EffectInvocation
	{

	}
	public class HandshakeReconnect : EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
		public int Attempts { get; set; }
		public PubnubError Reason { get; set; }
	}
	public class CancelHandshakeReconnect : EffectInvocation
	{

	}
	public class EmitStatus : EffectInvocation
	{
		private readonly PNStatusCategory statusCategory;
		public Action<PNStatus> AnnounceStatus { get; set; }
		public EmitStatus()
		{
		}
		public EmitStatus(PNStatusCategory status)
		{
			statusCategory = status;
		}

		public void Announce()
		{
			if (Handler != null)
			{
				PNStatus status = Handler.GetPNStatus();
				if (AnnounceStatus != null && status != null)
				{
					if (Handler is ReceivingEffectHandler<object> && status.StatusCode == 200)
					{
						//Ignore Announce for 200
						return;
					}
					System.Diagnostics.Debug.WriteLine($"Status Category = {status.Category} to be announced");
					AnnounceStatus(status);
				}
			}
		}
	}
	public class EmitMessages<T> : EffectInvocation
	{
		public Action<string> LogCallback { get; set; }
		public Action<PNMessageResult<object>> AnnounceMessage { get; set; }
		public EmitMessages(List<EventType> messages)
		{

		}
		public void Announce<T>()
		{
			Message<object>[] receiveMessages = ((ReceivingEffectHandler<object>)Handler).GetMessages();
			int messageCount = receiveMessages.Length;
			if (receiveMessages != null && receiveMessages.Length > 0)
			{
				for (int index = 0; index < receiveMessages.Length; index++)
				{
					LogCallback?.Invoke($"Received Message ({index + 1} of {receiveMessages.Length}) : {JsonConvert.SerializeObject(receiveMessages[index])}");
					if (receiveMessages[index].Channel.IndexOf("-pnpres") > 0)
					{
						var presenceData = JsonConvert.DeserializeObject<PresenceEvent>(receiveMessages[index].Payload.ToString());
					}
					else
					{
						LogCallback?.Invoke($"Message : {JsonConvert.SerializeObject(receiveMessages[index].Payload)}");
						PNMessageResult<object> messageResult = new PNMessageResult<object>();
						messageResult.Channel = receiveMessages[index].Channel;
						messageResult.Message = receiveMessages[index].Payload;
						AnnounceMessage?.Invoke(messageResult);
					}
				}
			}
		}
	}

	public class HandshakeFailed : EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
	}
	public class CancelHandshakeFailed : EffectInvocation
	{

	}
    #endregion
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
		CancelHandshakeFailure,
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
		public int Attempts { get; set; }

		public ExtendedState()
		{
			Channels = new List<string>();
			ChannelGroups = new List<string>();
			Timetoken = 0;
			Region = 0;
			Attempts = 0;
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
			if (PubnubUnitTest != null )
			{
				PubnubUnitTest.EventTypeList?.Clear();
			}
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
				if (PubnubUnitTest != null )
				{
					PubnubUnitTest.EventTypeList.Add(new KeyValuePair<string, string>("event", e.Name));
					PubnubUnitTest.Attempts = e.Attempts;
				}
				if (CurrentState != null && CurrentState.ExitList != null && CurrentState.ExitList.Count > 0)
				{
					foreach(var entry in CurrentState.ExitList)
					{
						PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", entry.Name));
						entry.Handler?.Cancel();
					}
				}
				if (CurrentState.EffectInvocationsList != null 
					&& CurrentState.EffectInvocationsList.ContainsKey(e.EventType)
					&& CurrentState.EffectInvocationsList[e.EventType].Count > 0) 
				{
					List<EffectInvocation> effectInvocationList = CurrentState.EffectInvocationsList[e.EventType];
					foreach (var effect in effectInvocationList) {
						PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", effect.Name));
						if (effect is EmitStatus)
						{
							((EmitStatus)effect).Announce();
						}
						else if (effect is EmitMessages<object>)
						{
							((EmitMessages<object>)effect).Announce<string>();
						}
						System.Diagnostics.Debug.WriteLine("Found effect " + effect.Effectype);
						Dispatcher.dispatch(effect.Effectype, this.Context);
						//if (e.EventType == effect.Effectype)
						//{
						//}
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
							PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", entry.Name));
							entry.Handler?.Start(Context);
						}
					}
					System.Diagnostics.Debug.WriteLine($"Next State = {CurrentState.StateType}; Transition = {e.EventType}");
					UpdateContext(e.EventType, e.EventPayload);
					//if (CurrentState.EffectInvocationsList[e.EventType].Count > 0) {
					//	foreach (var effect in CurrentState.EffectInvocationsList) {
					//		if (e.EventType == effect.Effectype)
					//		{
					//			System.Diagnostics.Debug.WriteLine("Found effect "+ effect.Effectype);
					//			Dispatcher.dispatch(effect.Effectype, this.Context);
					//		}
					//	}
					//}
				}
			}
		}

		public void Subscribe(List<string> channels, List<string>? channelGroups)
		{
			var evnt = new SubscriptionChanged();
			evnt.Name = "SUBSCRIPTION_CHANGED";
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
