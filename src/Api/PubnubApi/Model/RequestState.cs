using System;
using System.Net;
using System.Threading;

namespace PubnubApi
{
    public sealed class RequestState<T>
    {
        public DateTime? TimeQueued { get; internal set; }
        public CancellationTokenSource RequestCancellationTokenSource { get; internal set; }
        public TransportResponse Response { get; internal set; }
        public bool GotJsonResponse { get; internal set; }
        public PNOperationType ResponseType { get; internal set; }
        public string[] Channels { get; internal set; }
        public string[] ChannelGroups { get; internal set; }
        public bool Timeout { get; internal set; }
        public bool Reconnect { get; internal set; }
        public long Timetoken { get; internal set; }
        public int Region { get; internal set; }
        public PNCallback<T> PubnubCallback { get; internal set; }
        public bool UsePostMethod { get; internal set; }
        public bool UsePatchMethod { get; internal set; }
        public object EndPointOperation { get; internal set; }

        public RequestState()
        {
            PubnubCallback = null;
            RequestCancellationTokenSource = null;
            Response = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}
