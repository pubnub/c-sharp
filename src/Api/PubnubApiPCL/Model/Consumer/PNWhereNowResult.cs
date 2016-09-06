
namespace PubnubApi
{
    public class PNWhereNowResult
    {
        public PNWhereNowResult()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public string[] channels { get; set; }
        }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }
}
