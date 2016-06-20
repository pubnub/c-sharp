using System;

namespace PubnubApi
{
    internal class PubnubPresenceChannelGroupCallback
    {
        public Action<PresenceAck> PresenceRegularCallback;
        public Action<ConnectOrDisconnectAck> ConnectCallback;
        public Action<ConnectOrDisconnectAck> DisconnectCallback;
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
