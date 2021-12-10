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

        public string ChannelGroup { get; internal set; }
        public int Status { get; internal set; }
        public string Message { get; internal set; }
        public string Service { get; internal set; }
        public bool Error { get; internal set; }
    }
}
