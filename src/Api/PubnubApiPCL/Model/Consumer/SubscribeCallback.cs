using System;

namespace PubnubApi
{
    public class SubscribeCallback<T>
    {
        public Action<ConnectOrDisconnectAck> Connect { get; set; }
        public Action<ConnectOrDisconnectAck> Disconnect { get; set; }
        public Action<PNMessageResult<T>> Message { get; set; }
        public Action<PNPresenceEventResult> Presence { get; set; }
        public Action<PNPresenceEventResult> WildPresence { get; set; }
        public Action<PubnubClientError> Error { get; set; }
    }
}
