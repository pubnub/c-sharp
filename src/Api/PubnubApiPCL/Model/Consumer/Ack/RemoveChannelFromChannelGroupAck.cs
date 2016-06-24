using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class RemoveChannelFromChannelGroupAck
    {
        public RemoveChannelFromChannelGroupAck()
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
