
namespace PubnubApi
{
    public class GetAllNamespacesAck
    {
        public class Data
        {
            public Data()
            {
                this.SubKey = "";
            }

            public string[] NamespaceName { get; set; }
            public string SubKey { get; set; }
        }

        public GetAllNamespacesAck()
        {
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public Data Payload { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
