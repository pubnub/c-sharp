using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowResult
    {
        public int TotalChannels { get; set; }
        public int TotalOccupancy { get; set; }
        public Dictionary<string, PNHereNowChannelData> Channels { get; set; } = new Dictionary<string, PNHereNowChannelData>();
    }
}
