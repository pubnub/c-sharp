using System;
using System.Net;

namespace PubnubApi
{
    public sealed class RequestState<T>
    {
        //public Action<T> NonSubscribeRegularCallback { get; set; }
        //public Action<PNMessageResult<T>> SubscribeRegularCallback { get; set; }
        //public Action<PNPresenceEventResult> PresenceRegularCallback { get; set; }
        //public Action<ConnectOrDisconnectAck> ConnectCallback { get; set; }
        //public Action<PNPresenceEventResult> WildcardPresenceCallback { get; set; }
        //public Action<PubnubClientError> ErrorCallback { get; set; }
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }
        public PNOperationType ResponseType { get; set; }
        public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }
        public bool Timeout { get; set; }
        public bool Reconnect { get; set; }
        public long Timetoken { get; set; }
        public PNCallback<T> PubnubCallback { get; set; }

        public RequestState()
        {
            PubnubCallback = null;
            //SubscribeRegularCallback = null;
            //PresenceRegularCallback = null;
            //WildcardPresenceCallback = null;
            //ConnectCallback = null;
            Request = null;
            Response = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}
