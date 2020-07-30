using System;
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowChannelData
    {
        public string ChannelName { get; internal set; } = "";
        public int Occupancy { get; internal set; }
        public List<PNHereNowOccupantData> Occupants { get; internal set; } = new List<PNHereNowOccupantData>();
    }
}
