
namespace PubnubApi
{
    public class PublishAck
    {
        public PublishAck()
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
