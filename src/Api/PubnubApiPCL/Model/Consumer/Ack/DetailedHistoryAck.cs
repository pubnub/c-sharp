
namespace PubnubApi
{
    public class DetailedHistoryAck
    {
        public DetailedHistoryAck()
        {
            this.ChannelName = "";
        }

        public object[] Message;
        public long StartTimeToken;
        public long EndTimeToken;
        public string ChannelName { get; set; }
    }
}
