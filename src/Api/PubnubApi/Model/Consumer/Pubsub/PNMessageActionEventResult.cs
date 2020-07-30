using System;

namespace PubnubApi
{
    public class PNMessageActionEventResult
    {
        public string Event { get; internal set; }
        public long MessageTimetoken { get; internal set; }
        public long ActionTimetoken { get; internal set; }
        public PNMessageAction Action { get; internal set; }
        public string Uuid { get; internal set; }
        public string Channel { get; internal set; }
    }
}
