
namespace PubnubApi
{
    public class PNChannelGroupsAllResult
    {
        public class Data
        {
            public Data()
            {
                this.Namespace = "";
            }

            public string[] ChannelGroupName { get; set; }
            public string Namespace { get; set; }
        }

        public PNChannelGroupsAllResult()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
