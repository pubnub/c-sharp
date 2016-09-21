using System;

namespace PubnubApi
{
    public class PNMessageResult<T>
    {
        public T Message { get; set; }
        public string Channel { get; set; }
        private string Subscription;
        public long Timetoken { get; set; }
        public object UserMetadata { get; set; }
    }
}
