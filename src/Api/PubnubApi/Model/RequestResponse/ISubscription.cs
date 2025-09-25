using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi
{
    /// <summary>
    /// Represents an active subscription to PubNub channels or channel groups.
    /// Provides lifecycle management and event handling for real-time messages.
    /// </summary>
    public interface ISubscription : IDisposable
    {
        /// <summary>
        /// Gets whether this subscription is currently active and receiving messages.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets the channels this subscription is listening to.
        /// </summary>
        string[] Channels { get; }

        /// <summary>
        /// Gets the channel groups this subscription is listening to.
        /// </summary>
        string[] ChannelGroups { get; }

        /// <summary>
        /// Gets whether presence events are enabled for this subscription.
        /// </summary>
        bool PresenceEnabled { get; }

        /// <summary>
        /// Event raised when a message is received on any subscribed channel.
        /// </summary>
        event EventHandler<PNMessageEventArgs<object>> MessageReceived;

        /// <summary>
        /// Event raised when a presence event occurs on any subscribed channel.
        /// </summary>
        event EventHandler<PNPresenceEventArgs> PresenceEvent;

        /// <summary>
        /// Event raised when the subscription status changes (connected, disconnected, etc.).
        /// </summary>
        event EventHandler<PNStatusEventArgs> StatusChanged;

        /// <summary>
        /// Event raised when a signal is received on any subscribed channel.
        /// </summary>
        event EventHandler<PNSignalEventArgs<object>> SignalReceived;

        /// <summary>
        /// Event raised when an object event occurs (for App Context).
        /// </summary>
        event EventHandler<PNObjectEventArgs> ObjectEvent;

        /// <summary>
        /// Event raised when a message action event occurs.
        /// </summary>
        event EventHandler<PNMessageActionEventArgs> MessageActionEvent;

        /// <summary>
        /// Event raised when a file event occurs.
        /// </summary>
        event EventHandler<PNFileEventArgs> FileEvent;

        /// <summary>
        /// Asynchronously stops the subscription and releases resources.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the stop operation.</param>
        /// <returns>Task representing the stop operation.</returns>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronously stops the subscription and releases resources.
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Event arguments for message events.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    public class PNMessageEventArgs<T> : EventArgs
    {
        public PNMessageResult<T> Message { get; set; }
        public Pubnub Pubnub { get; set; }
    }

    /// <summary>
    /// Event arguments for presence events.
    /// </summary>
    public class PNPresenceEventArgs : EventArgs
    {
        public PNPresenceEventResult Presence { get; set; }
        public Pubnub Pubnub { get; set; }
    }

    /// <summary>
    /// Event arguments for status events.
    /// </summary>
    public class PNStatusEventArgs : EventArgs
    {
        public PNStatus Status { get; set; }
        public Pubnub Pubnub { get; set; }
    }

    /// <summary>
    /// Event arguments for signal events.
    /// </summary>
    /// <typeparam name="T">The type of the signal payload.</typeparam>
    public class PNSignalEventArgs<T> : EventArgs
    {
        public PNSignalResult<T> Signal { get; set; }
        public Pubnub Pubnub { get; set; }
    }

    /// <summary>
    /// Event arguments for object events.
    /// </summary>
    public class PNObjectEventArgs : EventArgs
    {
        public PNObjectEventResult ObjectEvent { get; set; }
        public Pubnub Pubnub { get; set; }
    }

    /// <summary>
    /// Event arguments for message action events.
    /// </summary>
    public class PNMessageActionEventArgs : EventArgs
    {
        public PNMessageActionEventResult MessageAction { get; set; }
        public Pubnub Pubnub { get; set; }
    }

    /// <summary>
    /// Event arguments for file events.
    /// </summary>
    public class PNFileEventArgs : EventArgs
    {
        public PNFileEventResult FileEvent { get; set; }
        public Pubnub Pubnub { get; set; }
    }
}