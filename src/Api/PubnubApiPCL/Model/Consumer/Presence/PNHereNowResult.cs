using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowResult
    {
        public int TotalChannels { get; set; }
        public int TotalOccupancy { get; set; }
        public Dictionary<string, PNHereNowChannelData> Channels { get; set; } = new Dictionary<string, PNHereNowChannelData>();
    }

    public class PNHereNowChannelData
    {
        public string ChannelName { get; set; } = "";
        public int Occupancy { get; set; }
        public List<PNHereNowOccupantData> Occupants { get; set; } = new List<PNHereNowOccupantData>();
    }

    public class PNHereNowOccupantData
    {
        public string Uuid { get; set; } = "";
        public object State { get; set; }
    }
}
