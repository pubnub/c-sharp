
namespace PubnubApi
{
    public class PNChannelGroupsAllChannelsResult
    {
        public class Data
        {
            public Data()
            {
                this.ChannelGroupName = "";
            }

            public string[] ChannelName { get; set; }
            public string ChannelGroupName { get; set; }
        }

        public PNChannelGroupsAllChannelsResult()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
