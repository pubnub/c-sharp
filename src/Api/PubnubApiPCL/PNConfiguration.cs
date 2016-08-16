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

        public int SubscribeTimeout { get; set; }

        public int PresenceHeartbeatTimeout
        {
            get
            {
                return presenceHeartbeatTimeout;
            }
        }

        public int PresenceHeartbeatInterval
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

        public string CiperKey { get; set; }

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

        public string SdkVersion
        {
            get
            {
                return "PubNub CSharp 4.0";
            }
        }

        //public LoggingMethod.Level LogVerbosity { get; set; }

        public IPubnubLog PubnubLog { get; set; }

        public IPubnubProxy PNProxy { get; set; }

        public PubnubErrorFilter.Level ErrorLevel { get; set; }

        public int ConnectTimeout { get; set; }

        public int NonSubscribeRequestTimeout { get; set; }

        public PNHeartbeatNotificationOption HeartbeatNotificationOption { get; set; }

        public string PushServiceName { get; set; }

        public bool EnableDebugForPushPublish { get; set; }

        public Collection<Uri> PushRemoteImageDomainUri { get; set; }

        //For publishing direct JSON string
        public bool EnableJsonEncodingForPublish { get; set; }

        public bool AddPayloadToPublishResponse { get; set; }

        public string FilterExpression { get; set; }

        public PNConfiguration()
        {
            this.Origin = "pubsub.pubnub.com";
            this.presenceHeartbeatTimeout = 300;
            this.uuid = Guid.NewGuid().ToString();
            this.NonSubscribeRequestTimeout = 10;
            this.SubscribeTimeout = 310;
            this.ConnectTimeout = 5;
            //this.LogVerbosity = LoggingMethod.Level.Off;
            this.Secure = true;
        }

        public PNConfiguration SetPresenceHeartbeatTimeoutWithCustomInterval(int timeout, int interval)
        {
            this.presenceHeartbeatTimeout = timeout;
            this.presenceHeartbeatInterval = interval;

            return this;
        }

        public PNConfiguration SetPresenceHeartbeatTimeout(int timeout)
        {
            this.presenceHeartbeatTimeout = timeout;

            return SetPresenceHeartbeatTimeoutWithCustomInterval(timeout, (timeout / 2) - 1);
        }


    }
}
