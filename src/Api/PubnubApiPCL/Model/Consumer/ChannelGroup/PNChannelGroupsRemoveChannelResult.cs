using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelGroupsRemoveChannelResult
    {
        public PNChannelGroupsRemoveChannelResult()
        {
            this.ChannelGroup = "";
            this.Message = "";
            this.Service = "";
        }

        public string ChannelGroup { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string Service { get; set; }
        public bool Error { get; set; }
    }
}
