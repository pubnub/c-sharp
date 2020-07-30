using System;
using System.Net;

namespace PubnubApi
{
    public sealed class RequestState<T>
    {
        public HttpWebRequest Request { get; internal set; }
        public HttpWebResponse Response { get; internal set; }
        public bool GotJsonResponse { get; internal set; }
        public PNOperationType ResponseType { get; internal set; }
        public string[] Channels { get; internal set; }
        public string[] ChannelGroups { get; internal set; }
        public bool Timeout { get; internal set; }
        public bool Reconnect { get; internal set; }
        public long Timetoken { get; internal set; }
        public PNCallback<T> PubnubCallback { get; internal set; }
        public bool UsePostMethod { get; internal set; }
        public bool UsePatchMethod { get; internal set; }
        public object EndPointOperation { get; internal set; }

        public RequestState()
        {
            PubnubCallback = null;
            Request = null;
            Response = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}
