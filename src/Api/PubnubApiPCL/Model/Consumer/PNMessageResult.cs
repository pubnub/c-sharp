using System;

namespace PubnubApi
{
    public class PNMessageResult<T>
    {
        public T Data { get; set; }
        public DateTime Time { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return Data.ToString();
        }
    }
}
