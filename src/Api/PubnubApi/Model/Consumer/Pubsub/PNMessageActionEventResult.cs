using System;

namespace PubnubApi
{
    public class PNMessageActionEventResult
    {
        public string Event { get; set; }
        public long MessageTimetoken { get; set; }
        public long ActionTimetoken { get; set; }
        public PNMessageAction Action { get; set; }
        public string Uuid { get; set; }
        public string Channel { get; set; }
    }
}
