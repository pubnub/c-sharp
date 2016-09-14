using System;

namespace PubnubApi
{
    public sealed class RequestState<T>
    {
        public Action<T> NonSubscribeRegularCallback { get; set; }
        public Action<PNMessageResult<T>> SubscribeRegularCallback { get; set; }
        public Action<PNPresenceEventResult> PresenceRegularCallback { get; set; }
        public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        public Action<PNPresenceEventResult> WildcardPresenceCallback { get; set; }
        public Action<PubnubClientError> ErrorCallback { get; set; }
        public PubnubWebRequest Request { get; set; }
        public PubnubWebResponse Response { get; set; }
        public ResponseType ResponseType { get; set; }
        public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }
        public bool Timeout { get; set; }
        public bool Reconnect { get; set; }
        public long Timetoken { get; set; }

        public RequestState()
        {
            SubscribeRegularCallback = null;
            PresenceRegularCallback = null;
            WildcardPresenceCallback = null;
            ConnectCallback = null;
            Request = null;
            Response = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}
