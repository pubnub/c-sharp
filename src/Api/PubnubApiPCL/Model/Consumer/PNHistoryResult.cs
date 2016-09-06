
namespace PubnubApi
{
    public class PNHistoryResult
    {
        public PNHistoryResult()
        {
            this.ChannelName = "";
        }

        public object[] Message { get; set; }
        public long StartTimeToken { get; set; }
        public long EndTimeToken { get; set; }
        public string ChannelName { get; set; }
    }
}
