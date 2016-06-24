
namespace PubnubApi
{
    public class RemoveNamespaceAck
    {
        public RemoveNamespaceAck()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
