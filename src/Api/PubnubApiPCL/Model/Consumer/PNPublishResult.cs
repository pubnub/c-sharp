
namespace PubnubApi
{
    public class PNPublishResult
    {
        public PNPublishResult()
        {
            this.StatusMessage = "";
            this.ChannelName = "";
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public long Timetoken { get; set; }
        public object Payload { get; set; }
    }
}
