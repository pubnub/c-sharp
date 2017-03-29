using System;
using System.Net;

namespace PubnubApi
{
    public sealed class RequestState<T>
    {
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }
        public bool GotJsonResponse { get; set; }
        public PNOperationType ResponseType { get; set; }
        public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }
        public bool Timeout { get; set; }
        public bool Reconnect { get; set; }
        public long Timetoken { get; set; }
        public PNCallback<T> PubnubCallback { get; set; }
        public bool UsePostMethod { get; set; }
        public object EndPointOperation { get; set; }

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
