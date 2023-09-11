using Newtonsoft.Json;
using PubnubApi.EndPoint;
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
		public virtual EffectInvocationType InvocationType { get; set; }
		public virtual IEffectInvocationHandler Handler { get; set; }
		public abstract bool IsManaged();
		public abstract bool IsCancelling();
	}
    public class ReceiveMessages: EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }

        public override bool IsManaged()
        {
            return true;
        }
        public override bool IsCancelling()
        {
            return false;
        }

    }
	public class CancelReceiveMessages : EffectInvocation
	{
		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return true;
        }
	}
	public class ReceiveReconnect: EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
		public int Attempts { get; set; }
		public PubnubError Reason { get; set; }
		public override bool IsManaged()
        {
            return true;
        }
        public override bool IsCancelling()
        {
            return false;
        }
	}
	public class CancelReceiveReconnect : EffectInvocation
	{
		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return true;
        }
	}
	public class Handshake : EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public override bool IsManaged()
        {
            return true;
        }
        public override bool IsCancelling()
        {
            return false;
        }
	}
	public class CancelHandshake : EffectInvocation
	{
		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return true;
        }
	}
	public class HandshakeReconnect : EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public SubscriptionCursor SubscriptionCursor { get; set; }
		public int Attempts { get; set; }
		public PubnubError Reason { get; set; }
		public override bool IsManaged()
        {
            return true;
        }
        public override bool IsCancelling()
        {
            return false;
        }
	}
	public class CancelHandshakeReconnect : EffectInvocation
	{
		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return true;
        }
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
					else if (Handler is ReceiveReconnectingEffectHandler<object> && status.StatusCode == 200)
					{
						//Ignore Announce for 200
						return;
					}
					System.Diagnostics.Debug.WriteLine($"Status Category = {status.Category} to be announced");
					AnnounceStatus(status);
				}
			}
		}

		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return false;
        }
	}
	public class EmitMessages<T> : EffectInvocation
	{
		public Action<string> LogCallback { get; set; }
		//public SubscribeOperation2<T> SubscribeOperation { get; set; }
		public Action<PNMessageResult<object>> AnnounceMessage { get; set; }
		public EmitMessages(List<EventType> messages)
		{

		}
		public void Announce<T>()
		{
		}

		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return false;
        }
	}

	public class HandshakeFailed : EffectInvocation
	{
		public List<string> Channels { get; set; }
		public List<string> ChannelGroups { get; set; }
		public override bool IsManaged()
        {
            return true;
        }
        public override bool IsCancelling()
        {
            return false;
        }
	}
	public class CancelHandshakeFailed : EffectInvocation
	{
		public override bool IsManaged()
        {
            return false;
        }
        public override bool IsCancelling()
        {
            return true;
        }
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
		ReceiveReconnectSuccess,
		ReceiveReconnectFailure,
		ReceiveReconnectGiveUp,
		HandshakeReconnect,
		CancelHandshakeReconnect,
		HandshakeReconnectSuccess,
		HandshakeReconnectFailure,
		HandshakeReconnectGiveUp,
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
			if (CurrentState != null) {
				State findState = States.Find((s) => s.StateType == CurrentState.StateType);
				StateType nextStateType;
				if (findState != null && findState.transitions != null && findState.transitions.TryGetValue(e.EventType, out nextStateType))
				{
					System.Diagnostics.Debug.WriteLine($"Current State = {CurrentState.StateType}; Transition = {e.EventType}");
					if (PubnubUnitTest != null )
					{
						PubnubUnitTest.EventTypeList.Add(new KeyValuePair<string, string>("event", e.Name));
						PubnubUnitTest.Attempts = e.Attempts;
					}
					if (findState != null)
					{
						if (findState.ExitList != null && findState.ExitList.Count > 0)
						{
							Dispatcher.dispatch(e.EventType, findState.ExitList, this.Context);
						}
						if (findState.EffectInvocationsList != null 
							&& findState.EffectInvocationsList.ContainsKey(e.EventType)
							&& findState.EffectInvocationsList[e.EventType].Count > 0) 
						{
							List<EffectInvocation> effectInvocationList = findState.EffectInvocationsList[e.EventType];
							if (effectInvocationList != null && effectInvocationList.Count > 0)
							{
								Dispatcher.dispatch(e.EventType, effectInvocationList, this.Context);
							}
						}
						
						CurrentState = States.Find((s) => s.StateType == nextStateType);
						UpdateContext(e.EventType, e.EventPayload);
						if (CurrentState.EntryList != null && CurrentState.EntryList.Count > 0)
						{
							Dispatcher.dispatch(e.EventType, CurrentState.EntryList, this.Context);
						}

						UpdateContext(e.EventType, e.EventPayload);
					}

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
			if (CurrentState != null)
			{
				CurrentState.EventType = eventType;
			}
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
            #region StateType.Handshaking
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
			#endregion

            #region HandshakeReconnecting Effect Invocations and Emit Status
            EmitStatus handshakeReconnectSuccessEmitStatus = new EmitStatus();
            handshakeReconnectSuccessEmitStatus.Name = "EMIT_STATUS";
            handshakeReconnectSuccessEmitStatus.Effectype = EventType.HandshakeReconnectSuccess;

			//TBD - Should we emit status/error on HandshakeReconnectFailure
			//EmitStatus handshakeReconnectFailureEmitStatus = new EmitStatus();
   //         handshakeReconnectFailureEmitStatus.Name = "EMIT_STATUS";
   //         handshakeReconnectFailureEmitStatus.Effectype = EventType.HandshakeReconnectFailure;

			EmitStatus handshakeReconnectGiveupEmitStatus = new EmitStatus();
			handshakeReconnectGiveupEmitStatus.Name = "EMIT_STATUS";
			handshakeReconnectGiveupEmitStatus.Effectype = EventType.HandshakeReconnectGiveUp;

            EffectInvocation handshakeReconnectInvocation = new HandshakeReconnect();
            handshakeReconnectInvocation.Name = "HANDSHAKE_RECONNECT";
            handshakeReconnectInvocation.Effectype = EventType.HandshakeReconnect;

            EffectInvocation cancelHandshakeReconnectInvocation = new CancelHandshakeReconnect();
            cancelHandshakeReconnectInvocation.Name = "CANCEL_HANDSHAKE_RECONNECT";
            cancelHandshakeReconnectInvocation.Effectype = EventType.CancelHandshakeReconnect;
            #endregion
            #region StateType.HandshakeReconnecting
            CreateState(StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.HandshakeReconnectFailure, StateType.HandshakeReconnecting)
    //            .On(EventType.HandshakeReconnectFailure, StateType.HandshakeReconnecting, new List<EffectInvocation>()
    //                        { 
    //                            handshakeReconnectFailureEmitStatus
    //                        }
				//)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.HandshakeReconnectGiveUp, StateType.HandshakeFailed, new List<EffectInvocation>()
                            { 
                                handshakeReconnectGiveupEmitStatus
                            }
                )
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
			#endregion

            #region HandshakeFailed Effect Invocations and Emit Status
            EffectInvocation handshakeFailedInvocation = new HandshakeFailed();
            handshakeFailedInvocation.Name = "HANDSHAKE_FAILED";
            handshakeFailedInvocation.Effectype = EventType.HandshakeFailure;

            EffectInvocation cancelHandshakeFailedInvocation = new CancelHandshakeFailed();
            cancelHandshakeFailedInvocation.Name = "CANCEL_HANDSHAKE_FAILED";
            cancelHandshakeFailedInvocation.Effectype = EventType.CancelHandshakeFailure;
            #endregion
            #region StateType.HandshakeFailed
            CreateState(StateType.HandshakeFailed)
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
            #endregion

            #region HandshakeStopped Effect Invocations and Emit Status
            #endregion
            #region StateType.HandshakeStopped
            CreateState(StateType.HandshakeStopped)
                .On(EventType.Reconnect, StateType.Handshaking);
			#endregion

            #region Receiving Effect Invocations and Emit Status
            EmitStatus receiveEmitStatus = new EmitStatus();
            receiveEmitStatus.Name = "EMIT_STATUS";
            receiveEmitStatus.Effectype = EventType.ReceiveSuccess;

            EmitMessages<T> receiveEmitMessages = new EmitMessages<T>(null);
            receiveEmitMessages.Name = "EMIT_EVENTS";
            receiveEmitMessages.Effectype = EventType.ReceiveMessages;

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
            #region StateType.Receiving
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
            #endregion

            #region ReceiveFailed Effect Invocations and Emit Status
            #endregion
            #region StateType.ReceiveFailed
            CreateState(StateType.ReceiveFailed)
				.On(EventType.SubscriptionChanged, StateType.Receiving)
				.On(EventType.SubscriptionRestored, StateType.Receiving)
                .On(EventType.Reconnect, StateType.Receiving);
            #endregion

            #region ReceiveReconnecting Effect Invocations and Emit Status
            EmitStatus receiveReconnectEmitStatus = new EmitStatus();
            receiveReconnectEmitStatus.Name = "RECONNECT_EMIT_STATUS";
            receiveReconnectEmitStatus.Effectype = EventType.ReceiveReconnectSuccess;

            EmitMessages<T> receiveReconnectEmitMessages = new EmitMessages<T>(null);
            receiveReconnectEmitMessages.Name = "RECEIVE_RECONNECT_EVENTS";
            receiveReconnectEmitMessages.Effectype = EventType.ReceiveMessages;

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
			cancelReceiveReconnect.Name = "CANCEL_RECEIVE_RECONNECT";
            cancelReceiveReconnect.Effectype = EventType.CancelReceiveReconnect;
            #endregion
            #region StateType.ReceiveReconnecting
            CreateState(StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionChanged, StateType.Receiving)
                .On(EventType.ReceiveReconnectFailure, StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .On(EventType.ReceiveReconnectSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                //receiveReconnectEmitStatus,
                                receiveReconnectEmitMessages
                            }
                )
                .On(EventType.Disconnect, StateType.ReceiveStopped)
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
            #endregion

            #region ReceiveStopped Effect Invocations and Emit Status
            #endregion
            #region StateType.ReceiveStopped
            CreateState(StateType.ReceiveStopped)
                .On(EventType.Reconnect, StateType.Receiving);
            #endregion

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
