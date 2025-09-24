using System;
using System.Collections.Generic;
using System.Linq;

namespace PubnubApi
{
    /// <summary>
    /// Request model for subscribe operations using the request/response API pattern.
    /// </summary>
    public class SubscribeRequest
    {
        /// <summary>
        /// Array of channels to subscribe to.
        /// </summary>
        public string[] Channels { get; set; }

        /// <summary>
        /// Array of channel groups to subscribe to.
        /// </summary>
        public string[] ChannelGroups { get; set; }

        /// <summary>
        /// The timetoken to use for the subscription. Default is -1 (use server time).
        /// </summary>
        public long Timetoken { get; set; } = -1;

        /// <summary>
        /// Whether to include presence events for the subscribed channels.
        /// </summary>
        public bool WithPresence { get; set; } = false;

        /// <summary>
        /// Additional query parameters to include in the subscription request.
        /// </summary>
        public Dictionary<string, object> QueryParameters { get; set; }

        /// <summary>
        /// Optional callback for handling received messages.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNMessageResult<object>> OnMessage { get; set; }

        /// <summary>
        /// Optional callback for handling presence events.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNPresenceEventResult> OnPresence { get; set; }

        /// <summary>
        /// Optional callback for handling status changes.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNStatus> OnStatus { get; set; }

        /// <summary>
        /// Optional callback for handling signal events.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNSignalResult<object>> OnSignal { get; set; }

        /// <summary>
        /// Optional callback for handling object events.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNObjectEventResult> OnObjectEvent { get; set; }

        /// <summary>
        /// Optional callback for handling message action events.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNMessageActionEventResult> OnMessageAction { get; set; }

        /// <summary>
        /// Optional callback for handling file events.
        /// Alternative to using events on ISubscription.
        /// </summary>
        public Action<Pubnub, PNFileEventResult> OnFile { get; set; }

        /// <summary>
        /// Validates that the request has at least one channel or channel group.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when both channels and channel groups are empty.</exception>
        public void Validate()
        {
            bool hasChannels = Channels != null && Channels.Length > 0 && Channels.Any(c => !string.IsNullOrWhiteSpace(c));
            bool hasChannelGroups = ChannelGroups != null && ChannelGroups.Length > 0 && ChannelGroups.Any(cg => !string.IsNullOrWhiteSpace(cg));

            if (!hasChannels && !hasChannelGroups)
            {
                throw new ArgumentException("Either Channels or ChannelGroups (or both) must be provided with at least one valid entry.");
            }

            // Clean up arrays to remove null/empty entries
            if (Channels != null)
            {
                Channels = Channels.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            }
            else
            {
                Channels = new string[0];
            }

            if (ChannelGroups != null)
            {
                ChannelGroups = ChannelGroups.Where(cg => !string.IsNullOrWhiteSpace(cg)).ToArray();
            }
            else
            {
                ChannelGroups = new string[0];
            }
        }
    }
}