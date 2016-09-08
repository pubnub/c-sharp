using System.Collections.Generic;

namespace PubnubApi
{
    public class PNSetStateResult
    {
        public PNSetStateResult()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public string[] ChannelName { get; set; }
        public string[] ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
