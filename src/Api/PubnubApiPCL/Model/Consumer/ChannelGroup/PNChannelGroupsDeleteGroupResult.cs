
namespace PubnubApi
{
    public class PNChannelGroupsDeleteGroupResult
    {
        public PNChannelGroupsDeleteGroupResult()
        {
            this.Message = "";
            this.Service = "";
        }

        public int Status { get; set; }
        public string Message { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
