using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class ReceiveReconnectRequestEventArgs : EventArgs 
	{
		public ExtendedState ExtendedState { get; set; }
		public Action<string> ReceiveReconnectResponseCallback { get; set; }
	}
	public class CancelReceiveReconnectRequestEventArgs : EventArgs 
	{ 
	}
	public class ReceiveReconnectingEffectHandler<T> : IEffectInvocationHandler
	{
		EventEmitter eventEmitter;
		private ExtendedState extendedState { get; set;}
		public Action<string> LogCallback { get; set; }
		public Action<PNStatus> AnnounceStatus { get; set; }
		public PNReconnectionPolicy ReconnectionPolicy { get; set; }
		public int MaxRetries { get; set; }
		private Message<object>[] receiveMessages { get; set; }
		private PNStatus pnStatus { get; set; }
		private int timerInterval;
		const int MINEXPONENTIALBACKOFF = 1;
		const int MAXEXPONENTIALBACKOFF = 25;
		const int INTERVAL = 3;

		public event EventHandler<ReceiveReconnectRequestEventArgs> ReceiveReconnectRequested;
		public event EventHandler<CancelReceiveReconnectRequestEventArgs> CancelReceiveReconnectRequested;
		System.Threading.Timer timer;
		protected virtual void OnReceiveReconnectRequested(ReceiveReconnectRequestEventArgs e)
        {
            EventHandler<ReceiveReconnectRequestEventArgs> handler = ReceiveReconnectRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }
		protected virtual void OnCancelReceiveReconnectRequested(CancelReceiveReconnectRequestEventArgs e)
        {
            EventHandler<CancelReceiveReconnectRequestEventArgs> handler = CancelReceiveReconnectRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        CancellationTokenSource cancellationTokenSource;
        public ReceiveReconnectingEffectHandler(EventEmitter emitter)
		{
			this.eventEmitter = emitter;
			cancellationTokenSource = new CancellationTokenSource();
		}

		public async void Start(ExtendedState context, EventType eventType)    
		{
			extendedState = context;
			await Task.Factory.StartNew(() => {	});
			if (cancellationTokenSource != null && cancellationTokenSource.Token.CanBeCanceled) {
				Cancel();
			}

			if (ReconnectionPolicy == PNReconnectionPolicy.EXPONENTIAL)
            {
				double numberForMath = extendedState.Attempts % 6;
                timerInterval = (int)(Math.Pow(2, numberForMath) - 1);
                if (timerInterval > MAXEXPONENTIALBACKOFF)
                {
                    timerInterval = MINEXPONENTIALBACKOFF;
                }
                else if (timerInterval < 1)
                {
                    timerInterval = MINEXPONENTIALBACKOFF;
                }
            }
            else if (ReconnectionPolicy == PNReconnectionPolicy.LINEAR)
            {
                timerInterval = INTERVAL;
            }
			else
			{
				timerInterval = -1;
			}
			LogCallback?.Invoke($"ReceiveReconnectingEffectHandler ReconnectionPolicy = {ReconnectionPolicy}; Interval = {timerInterval}");

			if (timer != null)
			{
				timer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			if (timerInterval != -1)
			{
				timer = new Timer(new TimerCallback(ReceiveReconnectTimerCallback), null, 
														(-1 == timerInterval) ? Timeout.Infinite : timerInterval * 1000, Timeout.Infinite);
			}
			else
			{
				PrepareFailurePNStatus(new ReceiveError() { Status = 400 });
				PrepareAndEmitReceiveReconnectGiveupEvent(null);
			}
		}

		private void ReceiveReconnectTimerCallback(object state)
		{
			LogCallback?.Invoke("ReceiveReconnectingEffectHandler Timer interval invoke");
			ReceiveReconnectRequestEventArgs args = new ReceiveReconnectRequestEventArgs();
			args.ExtendedState = extendedState;
			args.ReceiveReconnectResponseCallback = OnReceiveReconnectEffectResponseReceived;
			OnReceiveReconnectRequested(args);				
		}

		public void OnReceiveReconnectEffectResponseReceived(string json)
		{
			try
			{
				pnStatus = null;
				LogCallback?.Invoke($"OnReceiveReconnectEffectResponseReceived Json Response = {json}");
				var receivedResponse = JsonConvert.DeserializeObject<ReceiveingResponse<object>>(json);
				if (receivedResponse != null && receivedResponse.Timetoken != null)
				{
					receiveMessages = receivedResponse.Messages;
					
					ReceiveReconnectSuccess receiveReconnectSuccessEvent = new ReceiveReconnectSuccess();
					receiveReconnectSuccessEvent.SubscriptionCursor = new SubscriptionCursor();
					receiveReconnectSuccessEvent.SubscriptionCursor.Timetoken = receivedResponse.Timetoken.Timestamp;
					receiveReconnectSuccessEvent.SubscriptionCursor.Region = receivedResponse.Timetoken.Region;
					receiveReconnectSuccessEvent.EventPayload.Timetoken = receivedResponse.Timetoken.Timestamp;
					receiveReconnectSuccessEvent.EventPayload.Region = receivedResponse.Timetoken.Region;
					receiveReconnectSuccessEvent.EventType = EventType.ReceiveReconnectSuccess;
					receiveReconnectSuccessEvent.Name = "RECEIVE_RECONNECT_SUCCESS";
					LogCallback?.Invoke("OnReceiveReconnectEffectResponseReceived - EventType.ReceiveReconnectSuccess");
					
					pnStatus = new PNStatus();
					pnStatus.StatusCode = 200;
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Category = PNStatusCategory.PNConnectedCategory;
					pnStatus.Error = false;

					extendedState.Attempts = 0;
					
					eventEmitter.emit(receiveReconnectSuccessEvent);
				}
				else
				{
					ReceiveReconnectFailure receiveReconnectFailureEvent = new ReceiveReconnectFailure();
					receiveReconnectFailureEvent.Name = "RECEIVE_RECONNECT_FAILURE";
					receiveReconnectFailureEvent.EventType = EventType.ReceiveReconnectFailure;
					LogCallback?.Invoke("OnReceivingReconnectEffectResponseReceived - EventType.ReceiveReconnectFailure");

					pnStatus = new PNStatus();
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Error = true;


					var receiveReconnectError = JsonConvert.DeserializeObject<ReceiveError>(json);
					extendedState.Attempts++;
					PrepareFailurePNStatus(receiveReconnectError);

					if (MaxRetries != -1 && extendedState.Attempts > MaxRetries)
					{
						LogCallback?.Invoke($"Attempt: {extendedState.Attempts}; OnReceivingReconnectEffectResponseReceived - EventType.ReceiveReconnectGiveUp");
						PrepareAndEmitReceiveReconnectGiveupEvent(null);
					}
					else
					{
						LogCallback?.Invoke($"Attempt: {extendedState.Attempts}; OnReceivingReconnectEffectResponseReceived - EventType.ReceiveReconnectFailure");
						PrepareAndEmitReceiveReconnectFailureEvent(null);
					}
				}
			}
			catch (Exception ex)
			{
				extendedState.Attempts++;
				PrepareFailurePNStatus(new ReceiveError() { Status = 400 });

				if (MaxRetries != -1 && extendedState.Attempts > MaxRetries)
				{
					LogCallback?.Invoke($"Attempt: {extendedState.Attempts}; OnHandshakeReconnectEffectResponseReceived - EventType.ReceiveReconnectGiveUp");
					PrepareAndEmitReceiveReconnectGiveupEvent(null);
					
				}
				else
				{
					LogCallback?.Invoke($"Attempt: {extendedState.Attempts}; OnHandshakeReconnectEffectResponseReceived - EventType.ReceiveReconnectFailure");
					PrepareAndEmitReceiveReconnectFailureEvent(ex);
				}
			}
			finally
			{
				if (timer != null)
				{
					try
					{
						timer.Change(Timeout.Infinite, Timeout.Infinite);
					}
					catch { }
				}
			}
		}

		private void PrepareFailurePNStatus(ReceiveError error)
        {
			pnStatus = new PNStatus();
			pnStatus.StatusCode = (error != null && error.Status != 0) ? error.Status : 504;
			pnStatus.Operation = PNOperationType.PNSubscribeOperation;
			pnStatus.AffectedChannels = extendedState.Channels;
			pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
			pnStatus.Category = PNStatusCategory.PNNetworkIssuesCategory;
			pnStatus.Error = true;
		}

		private void PrepareAndEmitReceiveReconnectFailureEvent(Exception ex)
		{
			ReceiveReconnectFailure receiveReconnectFailureEvent = new ReceiveReconnectFailure();
			receiveReconnectFailureEvent.Name = "RECEIVE_RECONNECT_FAILURE";
			receiveReconnectFailureEvent.EventType = EventType.ReceiveReconnectFailure;
			receiveReconnectFailureEvent.Attempts = extendedState.Attempts;
			if (ex != null)
			{
				receiveReconnectFailureEvent.EventPayload.exception = ex;
			}

			eventEmitter.emit(receiveReconnectFailureEvent);
		}

		private void PrepareAndEmitReceiveReconnectGiveupEvent(Exception ex)
		{
			ReceiveReconnectGiveUp receiveReconnectGiveupEvent = new ReceiveReconnectGiveUp();
			receiveReconnectGiveupEvent.Name = "RECEIVE_RECONNECT_GIVEUP";
			receiveReconnectGiveupEvent.EventType = EventType.ReceiveReconnectGiveUp;
			receiveReconnectGiveupEvent.Attempts = extendedState.Attempts;
			if (ex != null)
			{
				receiveReconnectGiveupEvent.EventPayload.exception = ex;
			}

			eventEmitter.emit(receiveReconnectGiveupEvent);
		}

		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				LogCallback?.Invoke($"ReceiveReconnectEffectHandler - cancellationTokenSource - cancellion attempted.");
				cancellationTokenSource.Cancel();
			}
			if (timer != null)
			{
				timer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			LogCallback?.Invoke($"ReceiveReconnectEffectHandler - invoking OnCancelReceiveReconnectRequested.");
			CancelReceiveReconnectRequestEventArgs args = new CancelReceiveReconnectRequestEventArgs();
			OnCancelReceiveReconnectRequested(args);				
		}
        public void Run(ExtendedState context)
        {
            if (AnnounceStatus != null && pnStatus != null)
			{
				AnnounceStatus(pnStatus);
			}
        }
		public PNStatus GetPNStatus()
        {
            return pnStatus;
        }
	}
}
