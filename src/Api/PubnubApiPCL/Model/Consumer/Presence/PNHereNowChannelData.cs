using System;
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowChannelData
    {
        public string ChannelName { get; set; } = "";
        public int Occupancy { get; set; }
        public List<PNHereNowOccupantData> Occupants { get; set; } = new List<PNHereNowOccupantData>();
    }
}
