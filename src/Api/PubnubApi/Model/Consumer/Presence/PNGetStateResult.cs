using System.Collections.Generic;

namespace PubnubApi
{
    public class PNGetStateResult
    {
        //public string[] ChannelName { get; set; }
        //public string[] ChannelGroupName { get; set; }
        public Dictionary<string, object> StateByUUID { get; set; }
    }
}
