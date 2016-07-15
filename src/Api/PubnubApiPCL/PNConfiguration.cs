using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNConfiguration
    {
        private int _presenceHeartbeatTimeout;
        private int _presenceHeartbeatInterval;
        private string _uuid = "";
        private bool _enableProxy = false;

        public string Origin { get; set; }

        public int SubscribeTimeout { get; set; }

        public int PresenceHeartbeatTimeout
        {
            get
            {
                return _presenceHeartbeatTimeout;
            }
        }

        public int PresenceHeartbeatInterval
        {
            get
            {
                return _presenceHeartbeatInterval;
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
                return _uuid;
            }
            set
            {
                if (value != null && value.Trim().Length > 0)
                {
                    _uuid = value;
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

        public LoggingMethod.Level LogVerbosity { get; set; }

        public PubnubErrorFilter.Level ErrorLevel { get; set; }

        public int ConnectTimeout { get; set; }

        public int NonSubscribeRequestTimeout { get; set; }

        public PNHeartbeatNotificationOption HeartbeatNotificationOption { get; set; }

        public bool EnableProxy { get; set; }

        public bool EnableDebugForPushPublish { get; set; }

        //For publishing direct JSON string
        public bool EnableJsonEncodingForPublish { get; set; }

        public bool AddPayloadToPublishResponse { get; set; }

        public string FilterExpression { get; set; }

        public PNConfiguration()
        {
            this.Origin = "pubsub.pubnub.com";
            this._presenceHeartbeatTimeout = 300;
            this._uuid = Guid.NewGuid().ToString();
            this.NonSubscribeRequestTimeout = 10;
            this.SubscribeTimeout = 310;
            this.ConnectTimeout = 5;
            this.LogVerbosity = LoggingMethod.Level.Off;
            this.Secure = true;
        }

        public PNConfiguration SetPresenceHeartbeatTimeoutWithCustomInterval(int timeout, int interval)
        {
            this._presenceHeartbeatTimeout = timeout;
            this._presenceHeartbeatInterval = interval;

            return this;
        }

        public PNConfiguration SetPresenceHeartbeatTimeout(int timeout)
        {
            this._presenceHeartbeatTimeout = timeout;

            return SetPresenceHeartbeatTimeoutWithCustomInterval(timeout, (timeout / 2) - 1);
        }


    }
}
