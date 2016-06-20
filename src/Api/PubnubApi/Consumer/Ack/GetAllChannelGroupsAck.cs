
namespace PubnubApi
{
    public class GetAllChannelGroupsAck
    {
        public class Data
        {
            public Data()
            {
                this.Namespace = "";
            }

            public string[] ChannelGroupName;
            public string Namespace;
        }

        public GetAllChannelGroupsAck()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
