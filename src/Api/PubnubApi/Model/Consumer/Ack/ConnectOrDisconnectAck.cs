
namespace PubnubApi
{
    public class ConnectOrDisconnectAck
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public string ChannelGroupName { get; set; }
    }
}
