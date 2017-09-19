using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNConfiguration
    {
        private int presenceHeartbeatTimeout;
        private int presenceHeartbeatInterval;
        private string uuid = "";

        public string Origin { get; set; }

        public int PresenceTimeout
        {
            get
            {
                return presenceHeartbeatTimeout;
            }
            set
            {
                presenceHeartbeatTimeout = value;
                SetPresenceTimeoutWithCustomInterval(presenceHeartbeatTimeout, (presenceHeartbeatTimeout / 2) - 1);
            }
        }

        public int PresenceInterval
        {
            get
            {
                return presenceHeartbeatInterval;
            }
        }

        public bool Secure { get; set; }

        public string SubscribeKey { get; set; }

        public string PublishKey { get; set; }

        public string SecretKey { get; set; }

        public string CipherKey { get; set; }

        public string AuthKey { get; set; }

        public string Uuid
        {
            get
            {
                return uuid;
            }
            set
            {
                if (value != null && value.Trim().Length > 0)
                {
                    uuid = value;
                }
                else
                {
                    throw new ArgumentException("Missing or Incorrect Uuid value");
                }
            }
        }

        public PNLogVerbosity LogVerbosity { get; set; }

        public IPubnubLog PubnubLog { get; set; }

        public Proxy Proxy { get; set; }

        public int SubscribeTimeout { get; set; } //How long to keep the subscribe loop running before disconnect.

        public int NonSubscribeRequestTimeout { get; set; } //On non subscribe operations, how long to wait for server response.

        public PNHeartbeatNotificationOption HeartbeatNotificationOption { get; set; }

        public string FilterExpression { get; set; }

        public bool IncludeInstanceIdentifier { get; set; }

        public bool IncludeRequestIdentifier { get; set; }

        public PNReconnectionPolicy ReconnectionPolicy { get; set; } = PNReconnectionPolicy.NONE;

        public int RequestMessageCountThreshold { get; set; } = 100;

        public bool UseClassicHttpWebRequest { get; set; } = false;

        public bool EnableTelemetry { get; set; } = true;

        public PNConfiguration()
        {
            this.Origin = "ps.pndsn.com";
            this.presenceHeartbeatTimeout = 300;
            this.uuid = string.Format("pn-{0}", Guid.NewGuid().ToString());
            this.NonSubscribeRequestTimeout = 10;
            this.SubscribeTimeout = 310;
            this.LogVerbosity = PNLogVerbosity.NONE;
            this.CipherKey = "";
            this.PublishKey = "";
            this.SubscribeKey = "";
            this.SecretKey = "";
            this.Secure = true;
            this.ReconnectionPolicy = PNReconnectionPolicy.NONE;
            this.HeartbeatNotificationOption = PNHeartbeatNotificationOption.Failures;
            this.IncludeRequestIdentifier = true;
            this.IncludeInstanceIdentifier = false;
        }

        public PNConfiguration SetPresenceTimeoutWithCustomInterval(int timeout, int interval)
        {
            this.presenceHeartbeatTimeout = timeout;
            this.presenceHeartbeatInterval = interval;

            return this;
        }

    }
}
