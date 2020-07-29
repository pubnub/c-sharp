using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowResult
    {
        public int TotalChannels { get; internal set; }
        public int TotalOccupancy { get; internal set; }
        public Dictionary<string, PNHereNowChannelData> Channels { get; internal set; } = new Dictionary<string, PNHereNowChannelData>();
    }
}
