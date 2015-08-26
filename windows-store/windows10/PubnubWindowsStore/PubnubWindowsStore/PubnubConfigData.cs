using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubWindowsStore
{
    public class PubnubConfigData
    {
        public bool ssl;
        public bool resumeOnReconnect;
        public string publishKey = "";
        public string subscribeKey = "";
        public string cipherKey = "";
        public string secretKey = "";
        public string sessionUUID = "";
        public string origin = "";
        public string authKey = "";
        public bool hideErrorCallbackMessages;

        public int subscribeTimeout;
        public int nonSubscribeTimeout;
        public int maxRetries;
        public int retryInterval;
        public int localClientHeartbeatInterval;
        public int presenceHeartbeat;
        public int presenceHeartbeatInterval;
    }
}
