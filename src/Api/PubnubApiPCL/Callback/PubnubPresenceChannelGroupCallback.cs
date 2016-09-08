using System;

namespace PubnubApi
{
    internal class PubnubPresenceChannelGroupCallback
    {
        public Action<PresenceAck> PresenceRegularCallback { get; set; }
        public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        public Action<ConnectOrDisconnectAck> DisconnectCallback { get; set; }
        public Action<PubnubClientError> ErrorCallback;

        public PubnubPresenceChannelGroupCallback()
        {
            PresenceRegularCallback = null;
            ConnectCallback = null;
            DisconnectCallback = null;
            ErrorCallback = null;
        }
    }
}
