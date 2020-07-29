
namespace PubnubApi
{
    public class PNChannelGroupsDeleteGroupResult
    {
        public PNChannelGroupsDeleteGroupResult()
        {
            this.Message = "";
            this.Service = "";
        }

        public int Status { get; internal set; }
        public string Message { get; internal set; }
        public string Service { get; internal set; }
        public bool Error { get; internal set; }
    }
}
