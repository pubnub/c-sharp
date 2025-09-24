using System;
using System.Threading;
using System.Threading.Tasks;
using PubnubApi.EndPoint;

namespace PubnubApi
{
    /// <summary>
    /// Internal implementation of ISubscription that manages an active PubNub subscription.
    /// </summary>
    internal class SubscriptionImpl : ISubscription
    {
        private readonly Pubnub pubnub;
        private readonly SubscribeRequest request;
        private readonly SubscribeCallbackAdapter callbackAdapter;
        private readonly object subscribeOperation;
        private bool isActive;
        private bool disposed = false;

        public bool IsActive => isActive && !disposed;

        public string[] Channels { get; }

        public string[] ChannelGroups { get; }

        public bool PresenceEnabled { get; }

        // Events
        public event EventHandler<PNMessageEventArgs<object>> MessageReceived;
        public event EventHandler<PNPresenceEventArgs> PresenceEvent;
        public event EventHandler<PNStatusEventArgs> StatusChanged;
        public event EventHandler<PNSignalEventArgs<object>> SignalReceived;
        public event EventHandler<PNObjectEventArgs> ObjectEvent;
        public event EventHandler<PNMessageActionEventArgs> MessageActionEvent;
        public event EventHandler<PNFileEventArgs> FileEvent;

        public SubscriptionImpl(Pubnub pubnubInstance, SubscribeRequest subscribeRequest, object operation)
        {
            pubnub = pubnubInstance ?? throw new ArgumentNullException(nameof(pubnubInstance));
            request = subscribeRequest ?? throw new ArgumentNullException(nameof(subscribeRequest));
            subscribeOperation = operation ?? throw new ArgumentNullException(nameof(operation));

            Channels = request.Channels ?? new string[0];
            ChannelGroups = request.ChannelGroups ?? new string[0];
            PresenceEnabled = request.WithPresence;

            // Create the callback adapter that bridges to our events and request callbacks
            callbackAdapter = new SubscribeCallbackAdapter(this, request, pubnubInstance);

            // Add the adapter to the appropriate listener list based on operation type
            if (operation is ISubscribeOperation<object> subscribeOp)
            {
                subscribeOp.SubscribeListenerList.Add(callbackAdapter);
            }

            isActive = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (disposed)
                return;

            await Task.Run(() => Stop(), cancellationToken).ConfigureAwait(false);
        }

        public void Stop()
        {
            if (disposed)
                return;

            isActive = false;

            try
            {
                // Create an unsubscribe operation to cleanly disconnect
                if (Channels.Length > 0 || ChannelGroups.Length > 0)
                {
                    // Use reflection or type checking to handle both legacy and event engine modes
                    var unsubscribeOp = pubnub.Unsubscribe<object>();

                    if (Channels.Length > 0)
                        unsubscribeOp.Channels(Channels);

                    if (ChannelGroups.Length > 0)
                        unsubscribeOp.ChannelGroups(ChannelGroups);

                    unsubscribeOp.Execute();
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - we're stopping anyway
                System.Diagnostics.Debug.WriteLine($"Error during subscription stop: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Stop();

                // Remove the callback adapter from the listener list
                if (subscribeOperation is ISubscribeOperation<object> subscribeOp)
                {
                    subscribeOp.SubscribeListenerList.Remove(callbackAdapter);
                }
            }

            disposed = true;
        }

        internal void OnMessageReceived(Pubnub pn, PNMessageResult<object> message)
        {
            MessageReceived?.Invoke(this, new PNMessageEventArgs<object> { Pubnub = pn, Message = message });
        }

        internal void OnPresenceEvent(Pubnub pn, PNPresenceEventResult presence)
        {
            PresenceEvent?.Invoke(this, new PNPresenceEventArgs { Pubnub = pn, Presence = presence });
        }

        internal void OnStatusChanged(Pubnub pn, PNStatus status)
        {
            StatusChanged?.Invoke(this, new PNStatusEventArgs { Pubnub = pn, Status = status });
        }

        internal void OnSignalReceived(Pubnub pn, PNSignalResult<object> signal)
        {
            SignalReceived?.Invoke(this, new PNSignalEventArgs<object> { Pubnub = pn, Signal = signal });
        }

        internal void OnObjectEvent(Pubnub pn, PNObjectEventResult objectEvent)
        {
            ObjectEvent?.Invoke(this, new PNObjectEventArgs { Pubnub = pn, ObjectEvent = objectEvent });
        }

        internal void OnMessageActionEvent(Pubnub pn, PNMessageActionEventResult messageAction)
        {
            MessageActionEvent?.Invoke(this, new PNMessageActionEventArgs { Pubnub = pn, MessageAction = messageAction });
        }

        internal void OnFileEvent(Pubnub pn, PNFileEventResult fileEvent)
        {
            FileEvent?.Invoke(this, new PNFileEventArgs { Pubnub = pn, FileEvent = fileEvent });
        }

        /// <summary>
        /// Internal callback adapter that bridges from SubscribeCallback to ISubscription events.
        /// </summary>
        private class SubscribeCallbackAdapter : SubscribeCallback
        {
            private readonly SubscriptionImpl subscription;
            private readonly SubscribeRequest request;
            private readonly Pubnub pubnub;

            public SubscribeCallbackAdapter(SubscriptionImpl sub, SubscribeRequest req, Pubnub pn)
            {
                subscription = sub;
                request = req;
                pubnub = pn;
            }

            public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
            {
                // Route to request callback if provided
                if (request.OnMessage != null && message is PNMessageResult<object> objMessage)
                {
                    request.OnMessage(pubnub, objMessage);
                }

                // Route to subscription events
                if (message is PNMessageResult<object> objMsg)
                {
                    subscription.OnMessageReceived(pubnub, objMsg);
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
                // Route to request callback if provided
                request.OnPresence?.Invoke(pubnub, presence);

                // Route to subscription events
                subscription.OnPresenceEvent(pubnub, presence);
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                // Route to request callback if provided
                request.OnStatus?.Invoke(pubnub, status);

                // Route to subscription events
                subscription.OnStatusChanged(pubnub, status);
            }

            public override void Signal<T>(Pubnub pubnub, PNSignalResult<T> signal)
            {
                // Route to request callback if provided
                if (request.OnSignal != null && signal is PNSignalResult<object> objSignal)
                {
                    request.OnSignal(pubnub, objSignal);
                }

                // Route to subscription events
                if (signal is PNSignalResult<object> objSig)
                {
                    subscription.OnSignalReceived(pubnub, objSig);
                }
            }

            public override void ObjectEvent(Pubnub pubnub, PNObjectEventResult objectEvent)
            {
                // Route to request callback if provided
                request.OnObjectEvent?.Invoke(pubnub, objectEvent);

                // Route to subscription events
                subscription.OnObjectEvent(pubnub, objectEvent);
            }

            public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageAction)
            {
                // Route to request callback if provided
                request.OnMessageAction?.Invoke(pubnub, messageAction);

                // Route to subscription events
                subscription.OnMessageActionEvent(pubnub, messageAction);
            }

            public override void File(Pubnub pubnub, PNFileEventResult fileEvent)
            {
                // Route to request callback if provided
                request.OnFile?.Invoke(pubnub, fileEvent);

                // Route to subscription events
                subscription.OnFileEvent(pubnub, fileEvent);
            }
        }
    }
}