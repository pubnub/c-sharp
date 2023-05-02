using Newtonsoft.Json;
using PubnubApi.PubnubEventEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PubnubApi.PubnubEventEngine
{
	public class HandshakeReconnectRequestEventArgs : EventArgs 
	{
		public ExtendedState ExtendedState { get; set; }
		public Action<string> HandshakeReconnectResponseCallback { get; set; }
	}
    public class HandshakeReconnectEffectHandler : IEffectInvocationHandler
    {
        EventEmitter emitter;
		//public EffectInvocationType InvocationType { get; set; }
		private ExtendedState extendedState { get; set;}
		public Action<string> LogCallback { get; set; }
		public PNReconnectionPolicy ReconnectionPolicy { get; set; }
		private PNStatus pnStatus { get; set; }
		private int timerInterval;
		const int MINEXPONENTIALBACKOFF = 1;
		const int MAXEXPONENTIALBACKOFF = 25;
		const int INTERVAL = 3;

		public event EventHandler<HandshakeReconnectRequestEventArgs>? HandshakeReconnectRequested;
		System.Threading.Timer timer;
		protected virtual void OnHandshakeReconnectRequested(HandshakeReconnectRequestEventArgs e)
        {
            EventHandler<HandshakeReconnectRequestEventArgs>? handler = HandshakeReconnectRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        CancellationTokenSource? cancellationTokenSource;
        public HandshakeReconnectEffectHandler(EventEmitter emitter)
		{
			this.emitter = emitter;
			cancellationTokenSource = new CancellationTokenSource();
		}

        public async void Start(ExtendedState context)
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
			LogCallback?.Invoke($"HandshakeReconnectEffectHandler ReconnectionPolicy = {ReconnectionPolicy}; Interval = {timerInterval}");

			if (timer != null)
			{
				timer.Change(Timeout.Infinite, Timeout.Infinite);
			}
            timer = new Timer(new TimerCallback(HandshakeReconnectTimerCallback), null, 
                                                    (-1 == timerInterval) ? Timeout.Infinite : timerInterval * 1000, Timeout.Infinite);

		}

		private void HandshakeReconnectTimerCallback(object state)
		{
			LogCallback?.Invoke("HandshakeReconnectEffectHandler Timer interval invoke");
			HandshakeReconnectRequestEventArgs args = new HandshakeReconnectRequestEventArgs();
			args.ExtendedState = extendedState;
			args.HandshakeReconnectResponseCallback = OnHandshakeReconnectEffectResponseReceived;
			OnHandshakeReconnectRequested(args);				
		}

		public void OnHandshakeReconnectEffectResponseReceived(string json)
		{
			try
			{
				LogCallback?.Invoke($"OnHandshakeReconnectEffectResponseReceived Json Response = {json}");
				var handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(json);
				if (handshakeResponse != null)
				{
					HandshakeReconnectSuccess handshakeReconnectSuccessEvent = new HandshakeReconnectSuccess();
					handshakeReconnectSuccessEvent.SubscriptionCursor = new SubscriptionCursor();
					handshakeReconnectSuccessEvent.SubscriptionCursor.Timetoken = handshakeResponse.Timetoken?.Timestamp;
					handshakeReconnectSuccessEvent.SubscriptionCursor.Region = handshakeResponse.Timetoken?.Region;

					handshakeReconnectSuccessEvent.EventPayload.Timetoken = handshakeResponse.Timetoken?.Timestamp;
					handshakeReconnectSuccessEvent.EventPayload.Region = handshakeResponse.Timetoken?.Region;
					handshakeReconnectSuccessEvent.EventType = EventType.HandshakeReconnectSuccess;
					handshakeReconnectSuccessEvent.Name = "HANDSHAKE_RECONNECT_SUCCESS";
					LogCallback?.Invoke("OnHandshakeReconnectEffectResponseReceived - EventType.HandshakeReconnectSuccess");
					
					pnStatus = new PNStatus();
					pnStatus.StatusCode = 200;
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Category = PNStatusCategory.PNConnectedCategory;
					pnStatus.Error = false;
					
					extendedState.Attempts = 0;
					
					emitter.emit(handshakeReconnectSuccessEvent);
				}
				else
				{
					extendedState.Attempts++;
					HandshakeReconnectFailure handshakeReconnectFailureEvent = new HandshakeReconnectFailure();
					handshakeReconnectFailureEvent.Name = "HANDSHAKE_RECONNECT_FAILURE";
					handshakeReconnectFailureEvent.EventType = EventType.HandshakeReconnectFailure;
					LogCallback?.Invoke($"Attempt: {extendedState.Attempts}; OnHandshakeReconnectEffectResponseReceived - EventType.HandshakeReconnectFailure");

					pnStatus = new PNStatus();
					pnStatus.Operation = PNOperationType.PNSubscribeOperation;
					pnStatus.AffectedChannels = extendedState.Channels;
					pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
					pnStatus.Category = PNStatusCategory.PNNetworkIssuesCategory;
					pnStatus.Error = true;

					emitter.emit(handshakeReconnectFailureEvent);
				}
			}
			catch (Exception ex)
			{
				extendedState.Attempts++;
				LogCallback?.Invoke($"Attempt: {extendedState.Attempts} - OnHandshakeReconnectEffectResponseReceived EXCEPTION - {ex}");
				HandshakeReconnectFailure handshakeReconnectFailureEvent = new HandshakeReconnectFailure();
				handshakeReconnectFailureEvent.Name = "HANDSHAKE_RECONNECT_FAILURE";
				handshakeReconnectFailureEvent.EventType = EventType.HandshakeReconnectFailure;
				handshakeReconnectFailureEvent.EventPayload.exception = ex;

				pnStatus = new PNStatus();
				pnStatus.Operation = PNOperationType.PNSubscribeOperation;
				pnStatus.AffectedChannels = extendedState.Channels;
				pnStatus.AffectedChannelGroups = extendedState.ChannelGroups;
				pnStatus.Category = PNStatusCategory.PNNetworkIssuesCategory;
				pnStatus.Error = true;

				emitter.emit(handshakeReconnectFailureEvent);
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
			
			//emitter.emit(json, true, 0);
		}
		public void Cancel()
		{
			if (cancellationTokenSource != null)
			{
				LogCallback?.Invoke($"HandshakeReconnectEffectHandler - cancellationTokenSource - cancellion attempted.");
				cancellationTokenSource.Cancel();
			}
		}

        public PNStatus GetPNStatus()
        {
            return pnStatus;
        }
    }
}
