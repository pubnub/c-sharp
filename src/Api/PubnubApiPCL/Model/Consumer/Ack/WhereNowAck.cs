
namespace PubnubApi
{
    public class WhereNowAck
    {
        public WhereNowAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public string[] channels;
        }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }
}
