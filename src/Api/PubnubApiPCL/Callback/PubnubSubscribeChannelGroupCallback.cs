using System;

namespace PubnubApi
{
    public class PubnubSubscribeChannelGroupCallback<T>
    {
        public Action<Message<T>> SubscribeRegularCallback;
        public Action<ConnectOrDisconnectAck> ConnectCallback;
        public Action<ConnectOrDisconnectAck> DisconnectCallback;
        public Action<PresenceAck> WildcardPresenceCallback;
        public Action<PubnubClientError> ErrorCallback;

        public PubnubSubscribeChannelGroupCallback()
        {
            SubscribeRegularCallback = null;
            ConnectCallback = null;
            DisconnectCallback = null;
            WildcardPresenceCallback = null;
            ErrorCallback = null;
        }
    }
}
