using System;

namespace PubnubApi
{
    public class PNConfiguration
    {
        private int _presenceHeartbeatTimeout;
        private int _presenceHeartbeatInterval;
        private string _uuid = "";

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

        public LoggingMethod.Level LogVerbosity { get; set; }

        public int ConnectTimeout { get; set; }

        public int NonSubscribeRequestTimeout { get; set; }

        public PNHeartbeatNotificationOption HeartbeatNotificationOption { get; set; }

        public string FilterExpression { get; set; }

        public PNConfiguration()
        {
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
