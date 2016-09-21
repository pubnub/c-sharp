using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNStatus
    {
        public PNStatusCategory Category;
        public PNErrorData ErrorData;
        public bool Error;

        public int StatusCode;
        public PNOperationType Operation;

        public bool TlsEnabled;

        public string Uuid;
        public string AuthKey;
        public string Origin;
        public object ClientRequest;

        // send back channel, channel groups that were affected by this operation
        public List<string> AffectedChannels;
        public List<string> AffectedChannelGroups;

        //private Endpoint executedEndpoint;


        //public void retry()
        //{
        //    executedEndpoint.retry();
        //}

    }
}
