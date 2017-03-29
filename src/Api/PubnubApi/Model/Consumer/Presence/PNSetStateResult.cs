using System.Collections.Generic;

namespace PubnubApi
{
    public class PNSetStateResult
    {
        //public string[] ChannelName { get; set; }
        //public string[] ChannelGroupName { get; set; }
        public Dictionary<string, object> State { get; set; }
    }
}
