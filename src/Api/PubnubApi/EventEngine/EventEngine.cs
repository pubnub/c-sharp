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
		private EventEngine pnEventEngine = null;

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
			if (States.Find(s=> s.StateType == type) != null) 
			{
				throw new InvalidOperationException($"StateType = {type} already exist.");
			}
			States.Add(newState);
			return newState;
		}

		public void Transition(Event e)
		{
			StateType nextStateType;
			if (CurrentState != null) {
				State findState = States.Find((s) => s.StateType == CurrentState.StateType);
				System.Diagnostics.Debug.WriteLine($"Current State = {CurrentState.StateType}; Transition = {e.EventType}");
				if (PubnubUnitTest != null )
				{
					PubnubUnitTest.EventTypeList.Add(new KeyValuePair<string, string>("event", e.Name));
					PubnubUnitTest.Attempts = e.Attempts;
				}
				if (findState != null && findState.ExitList != null && findState.ExitList.Count > 0)
				{
					foreach(var entry in CurrentState.ExitList)
					{
						PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", entry.Name));
						entry.Handler?.Cancel();
					}
				}
				if (findState.EffectInvocationsList != null 
					&& findState.EffectInvocationsList.ContainsKey(e.EventType)
					&& findState.EffectInvocationsList[e.EventType].Count > 0) 
				{
					List<EffectInvocation> effectInvocationList = findState.EffectInvocationsList[e.EventType];
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
				findState.EventType = e.EventType;
				CurrentState = findState;
				UpdateContext(e.EventType, e.EventPayload);
				if (findState != null)
				{
					if (findState.EntryList != null && findState.EntryList.Count > 0)
					{
						foreach(var entry in findState.EntryList)
						{
							PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", entry.Name));
							entry.Handler?.Start(Context);
						}
					}
					CurrentState = NextState();
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

		public State NextState() 
		{
			State nextState = null;
			if (CurrentState != null)
			{
				StateType nextStateType;
				State findState = States.Find((s) => s.StateType == CurrentState.StateType);
				if (findState != null && findState.transitions != null && findState.transitions.ContainsKey(CurrentState.EventType))
				{
					nextStateType = findState.transitions[CurrentState.EventType];
					nextState = States.Find((s) => s.StateType == nextStateType);
				}
			}
			return nextState;
		}

		public void Setup<T>(PNConfiguration config)
		{
			CreateState(StateType.Unsubscribed)
				.On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.SubscriptionRestored, StateType.Receiving);

            #region Handshake Effect Invocations and Emit Status
            EmitStatus handshakeSuccessEmitStatus = new EmitStatus();
            handshakeSuccessEmitStatus.Name = "EMIT_STATUS";
            handshakeSuccessEmitStatus.Effectype = EventType.HandshakeSuccess;

            EffectInvocation handshakeInvocation = new Handshake();
            handshakeInvocation.Name = "HANDSHAKE";
            handshakeInvocation.Effectype = EventType.Handshake;

            EffectInvocation cancelHandshakeInvocation = new CancelHandshake();
            cancelHandshakeInvocation.Name = "CANCEL_HANDSHAKE";
            cancelHandshakeInvocation.Effectype = EventType.CancelHandshake;
            #endregion
            CreateState(StateType.Handshaking)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.HandshakeSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                handshakeSuccessEmitStatus
                            }
                )
                .On(EventType.HandshakeFailure, StateType.HandshakeReconnecting)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                handshakeInvocation 
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelHandshakeInvocation 
                            }
                );

            #region HandshakeReconnecting Effect Invocations and Emit Status
            EmitStatus handshakeReconnectSuccessEmitStatus = new EmitStatus();
            handshakeReconnectSuccessEmitStatus.Name = "EMIT_STATUS";
            handshakeReconnectSuccessEmitStatus.Effectype = EventType.HandshakeReconnectSuccess;

            EffectInvocation handshakeReconnectInvocation = new HandshakeReconnect();
            handshakeReconnectInvocation.Name = "HANDSHAKE_RECONNECT";
            handshakeReconnectInvocation.Effectype = EventType.HandshakeReconnect;

            EffectInvocation cancelHandshakeReconnectInvocation = new CancelHandshakeReconnect();
            cancelHandshakeReconnectInvocation.Name = "CANCEL_HANDSHAKE_RECONNECT";
            cancelHandshakeReconnectInvocation.Effectype = EventType.CancelHandshakeReconnect;
            #endregion
            CreateState(StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.HandshakeReconnecting)
                .On(EventType.HandshakeReconnectFailure, StateType.HandshakeReconnecting)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.HandshakeReconnectGiveUp, StateType.HandshakeFailed)
                .On(EventType.HandshakeReconnectSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                handshakeReconnectSuccessEmitStatus
                            }
                )
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                handshakeReconnectInvocation
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelHandshakeReconnectInvocation 
                            }
                );

            #region HandshakeFailed Effect Invocations and Emit Status
            EffectInvocation handshakeFailedInvocation = new HandshakeFailed();
            handshakeFailedInvocation.Name = "HANDSHAKE_FAILED";
            handshakeFailedInvocation.Effectype = EventType.HandshakeFailure;

            EffectInvocation cancelHandshakeFailedInvocation = new CancelHandshakeFailed();
            cancelHandshakeReconnectInvocation.Name = "CANCEL_HANDSHAKE_FAILED";
            cancelHandshakeReconnectInvocation.Effectype = EventType.CancelHandshakeFailure;
            #endregion
            CreateState(StateType.HandshakeFailed)
                .On(EventType.HandshakeReconnectRetry, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.Reconnect, StateType.Handshaking)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                handshakeFailedInvocation
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelHandshakeFailedInvocation 
                            }
                );

            #region HandshakeStopped Effect Invocations and Emit Status
            #endregion
            CreateState(StateType.HandshakeStopped)
                .On(EventType.Reconnect, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.HandshakeSuccess, StateType.Receiving)
                .On(EventType.HandshakeFailure, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionRestored, StateType.Receiving);

            #region Receiving Effect Invocations and Emit Status
            EmitStatus receiveEmitStatus = new EmitStatus();
            receiveEmitStatus.Name = "EMIT_STATUS";
            receiveEmitStatus.Effectype = EventType.ReceiveSuccess;

            EmitMessages<T> receiveEmitMessages = new EmitMessages<T>(null);
            receiveEmitMessages.Name = "EMIT_EVENTS";
            receiveEmitMessages.Effectype = EventType.ReceiveSuccess;

            EmitStatus receiveDisconnectEmitStatus = new EmitStatus();
            receiveDisconnectEmitStatus.Name = "EMIT_STATUS";
            receiveDisconnectEmitStatus.Effectype = EventType.Disconnect;

            EffectInvocation receiveMessagesInvocation = new ReceiveMessages();
            receiveMessagesInvocation.Name = "RECEIVE_EVENTS";
            receiveMessagesInvocation.Effectype = EventType.ReceiveMessages;

            EffectInvocation cancelReceiveMessages = new CancelReceiveMessages();
            cancelReceiveMessages.Name = "CANCEL_RECEIVE_EVENTS";
            cancelReceiveMessages.Effectype = EventType.CancelReceiveMessages;
            #endregion
            CreateState(StateType.Receiving)
                .On(EventType.SubscriptionChanged, StateType.Receiving)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .On(EventType.ReceiveSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                receiveEmitStatus,
                                receiveEmitMessages
                            }
                )
                .On(EventType.Disconnect, StateType.ReceiveStopped, new List<EffectInvocation>()
                            { 
                                 receiveDisconnectEmitStatus
                            }
                )
                .On(EventType.ReceiveFailure, StateType.ReceiveReconnecting)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                receiveMessagesInvocation
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelReceiveMessages
                            }
                );

            #region ReceiveReconnecting Effect Invocations and Emit Status
            EmitStatus receiveReconnectEmitStatus = new EmitStatus();
            receiveReconnectEmitStatus.Name = "RECONNECT_EMIT_STATUS";
            receiveEmitStatus.Effectype = EventType.ReceiveReconnectSuccess;

            EmitMessages<T> receiveReconnectEmitMessages = new EmitMessages<T>(null);
            receiveReconnectEmitMessages.Name = "RECEIVE_RECONNECT_EVENTS";
            receiveReconnectEmitMessages.Effectype = EventType.ReceiveReconnectSuccess;

            EmitStatus receiveReconnectDisconnectEmitStatus = new EmitStatus();
            receiveReconnectDisconnectEmitStatus.Name = "RECONNECT_DISCONNECT_STATUS";
            receiveReconnectDisconnectEmitStatus.Effectype = EventType.Disconnect;

            EmitStatus receiveReconnectGiveupEmitStatus = new EmitStatus();
            receiveReconnectGiveupEmitStatus.Name = "RECONNECT_GIVEUP_STATUS";
            receiveReconnectGiveupEmitStatus.Effectype = EventType.ReceiveReconnectGiveUp;

            EffectInvocation receiveReconnectInvocation = new ReceiveReconnect();
            receiveReconnectInvocation.Name = "RECEIVE_RECONNECT";
            receiveReconnectInvocation.Effectype = EventType.ReceiveReconnect;

            EffectInvocation cancelReceiveReconnect = new CancelReceiveReconnect();
            cancelReceiveReconnect.Effectype = EventType.CancelReceiveMessages;
            #endregion
            CreateState(StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionChanged, StateType.ReceiveReconnecting)
                .On(EventType.ReceiveReconnectFailure, StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionRestored, StateType.ReceiveReconnecting)
                .On(EventType.ReceiveReconnectSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                receiveReconnectEmitStatus,
                                receiveReconnectEmitMessages
                            }
                )
                .On(EventType.Disconnect, StateType.ReceiveStopped, new List<EffectInvocation>()
                            { 
                                receiveReconnectDisconnectEmitStatus
                            }
                )
                .On(EventType.ReceiveReconnectGiveUp, StateType.ReceiveFailed, new List<EffectInvocation>()
                            { 
                                receiveReconnectGiveupEmitStatus
                            }
                )
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                receiveReconnectInvocation
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelReceiveReconnect
                            }
                );
			System.Diagnostics.Debug.WriteLine("EventEngine Setup done.");
		}

		//public void SetCurrentStateType(StateType stateType, EventType eventType)
		//{
		//	State nextState = null;
		//	StateType nextStateType;
		//	IEnumerable<State> eventTypeStates = States.Where(s => s.StateType == stateType && s.EventType == eventType);
		//	foreach (State state in eventTypeStates)
		//	{
		//		if (state.transitions.ContainsKey(eventType))
		//		{
		//			nextStateType = state.transitions[eventType];
		//		}
		//	}
		//}
	}
}
