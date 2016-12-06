using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PNStatusCategory Category { get; set; }

        public PNErrorData ErrorData { get; set; }
        public bool Error { get; set; }

        public int StatusCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNOperationType Operation { get; set; }

        public bool TlsEnabled { get; set; }

        public string Uuid { get; set; }
        public string AuthKey { get; set; }
        public string Origin { get; set; }
        public object ClientRequest { get; set; }

        // send back channel, channel groups that were affected by this operation
        public List<string> AffectedChannels { get; set; } = new List<string>();
        public List<string> AffectedChannelGroups { get; set; } = new List<string>();

        //private Endpoint executedEndpoint;


        //public void retry()
        //{
        //    executedEndpoint.retry();
        //}

    }
}
