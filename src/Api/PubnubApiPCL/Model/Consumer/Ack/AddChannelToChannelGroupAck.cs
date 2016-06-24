
namespace PubnubApi
{
    public class AddChannelToChannelGroupAck
    {
        public AddChannelToChannelGroupAck()
        {
            this.ChannelGroupName = "";
            this.StatusMessage = "";
            this.Service = "";
        }

        public string ChannelGroupName { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
