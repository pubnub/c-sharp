using System.Collections.Generic;

namespace PubnubApi
{
    public class GetUserStateAck
    {
        public GetUserStateAck()
        {
            this.StatusMessage = "";
            this.Service = "";
            this.UUID = "";
        }

        public string[] ChannelName { get; set; }
        public string[] ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public string UUID { get; set; }
    }
}
