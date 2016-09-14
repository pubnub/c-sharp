using System;

namespace PubnubApi
{
    public class PubnubSubscribeChannelGroupCallback<T>
    {
        public Action<PNMessageResult<T>> SubscribeRegularCallback { get; set; }
        public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        public Action<ConnectOrDisconnectAck> DisconnectCallback { get; set; }
        public Action<PNPresenceEventResult> WildcardPresenceCallback { get; set; }
        public Action<PubnubClientError> ErrorCallback { get; set; }

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
