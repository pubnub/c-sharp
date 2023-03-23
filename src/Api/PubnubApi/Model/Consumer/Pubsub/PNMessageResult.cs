using System;

namespace PubnubApi
{
    public class PNMessageResult<T>
    {
        public T Message { get; internal set; }
        public string Channel { get; internal set; }
        public string Subscription { get; internal set; }
        public long Timetoken { get; internal set; }
        public object UserMetadata { get; internal set; }
        public string Publisher { get; internal set; }
        public string Type { get; internal set;}
        public string SpaceId { get; internal set; }
    }
}
