using System;

namespace PubnubApi
{
    public sealed class RequestState<T>
    {
        public Action<T> NonSubscribeRegularCallback;
        public Action<Message<T>> SubscribeRegularCallback;
        public Action<PresenceAck> PresenceRegularCallback;
        public Action<ConnectOrDisconnectAck> ConnectCallback;
        public Action<PresenceAck> WildcardPresenceCallback;
        public Action<PubnubClientError> ErrorCallback;
        public PubnubWebRequest Request;
        public PubnubWebResponse Response;
        public ResponseType ResponseType;
        public string[] Channels;
        public string[] ChannelGroups;
        public bool Timeout;
        public bool Reconnect;
        public long Timetoken;

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
