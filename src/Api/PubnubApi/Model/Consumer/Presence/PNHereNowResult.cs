using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowResult
    {
        public int TotalChannels { get; internal set; }
        public int TotalOccupancy { get; internal set; }
        public Dictionary<string, PNHereNowChannelData> Channels { get; internal set; } = new Dictionary<string, PNHereNowChannelData>();
        
        /// <summary>
        /// Useful for pagination.
        /// Gives value to be set to fetch next page.
        /// Set this value as 'offset' parameter in next HereNow call
        /// to fetch next page.
        /// </summary>
        public int? NextOffset { get; internal set; }
    }
}
