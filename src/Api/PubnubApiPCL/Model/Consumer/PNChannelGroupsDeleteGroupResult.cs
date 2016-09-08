
namespace PubnubApi
{
    public class PNChannelGroupsDeleteGroupResult
    {
        //public class Data
        //{
        //    public Data()
        //    {
        //        this.ChannelGroupName = "";
        //    }

        //    public string[] ChannelName;
        //    public string ChannelGroupName;
        //}

        public PNChannelGroupsDeleteGroupResult()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        //public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
