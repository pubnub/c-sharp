using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal class PubnubPresenceChannelCallback
    {
        public Action<PNPresenceEventResult> PresenceRegularCallback { get; set; }
        public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        public Action<ConnectOrDisconnectAck> DisconnectCallback { get; set; }
        public Action<PubnubClientError> ErrorCallback { get; set; }

        public PubnubPresenceChannelCallback()
        {
            PresenceRegularCallback = null;
            ConnectCallback = null;
            DisconnectCallback = null;
            ErrorCallback = null;
        }
    }
}
