
namespace PubnubApi
{
    public class GetChannelGroupChannelsAck
    {
        public class Data
        {
            public Data()
            {
                this.ChannelGroupName = "";
            }

            public string[] ChannelName;
            public string ChannelGroupName;
        }

        public GetChannelGroupChannelsAck()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
