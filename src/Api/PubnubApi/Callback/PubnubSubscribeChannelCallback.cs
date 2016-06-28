using System;

namespace PubnubApi
{
    public class PubnubSubscribeChannelCallback<T>
    {
        public Action<Message<T>> SubscribeRegularCallback;
        public Action<ConnectOrDisconnectAck> ConnectCallback;
        public Action<ConnectOrDisconnectAck> DisconnectCallback;
        public Action<PresenceAck> WildcardPresenceCallback;
        public Action<PubnubClientError> ErrorCallback;

        public PubnubSubscribeChannelCallback()
        {
            SubscribeRegularCallback = null;
            ConnectCallback = null;
            DisconnectCallback = null;
            WildcardPresenceCallback = null;
            ErrorCallback = null;
        }
    }
}
