using System;

namespace PubnubApi
{
    public class PubnubSubscribeChannelCallback<T>
    {
        public Action<Message<T>> SubscribeRegularCallback { get; set; }
        public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        public Action<ConnectOrDisconnectAck> DisconnectCallback { get; set; }
        public Action<PresenceAck> WildcardPresenceCallback { get; set; }
        public Action<PubnubClientError> ErrorCallback { get; set; }

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
