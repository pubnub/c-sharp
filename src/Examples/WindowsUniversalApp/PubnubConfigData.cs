using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUniversalApp
{
    public class PubnubConfigData
    {
        public bool ssl { get; set; }
        public string publishKey { get; set; } = "";
        public string subscribeKey { get; set; } = "";
        public string cipherKey { get; set; } = "";
        public string secretKey { get; set; } = "";
        public string sessionUUID { get; set; } = "";
        public string origin { get; set; } = "";
        public string authKey { get; set; } = "";

        public int subscribeTimeout { get; set; }
        public int nonSubscribeTimeout { get; set; }
        public int maxRetries { get; set; }
        public int retryInterval { get; set; }
        public int localClientHeartbeatInterval { get; set; }
        public int presenceHeartbeat { get; set; }
        public int presenceHeartbeatInterval { get; set; }
    }
}
