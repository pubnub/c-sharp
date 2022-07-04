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
        private string _uuid = "";
        private UserId _userId;
        private bool usingDeprecatedUuid = false;

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

        [Obsolete("Uuid is deprecated, please use UserId instead.")]

        public string Uuid
        {
            get
            {
                return _uuid;
            }
            set
            {
                if (_userId != null && !usingDeprecatedUuid)
                {
                    throw new ArgumentException("Either UserId or Uuid can be used. Not both.");
                }

                if (value != null && value.Trim().Length > 0)
                {
                    _uuid = value;
                    _userId = new UserId(value);
                }
                else
                {
                    throw new ArgumentException("Missing or Incorrect Uuid value");
                }
            }
        }

        public UserId UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                if (_uuid != null && usingDeprecatedUuid)
                {
                    throw new ArgumentException("Either UserId or Uuid can be used. Not both.");
                }

                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    _userId = value;
                    _uuid = value.ToString();
                }
                else
                {
                    throw new ArgumentException("Missing or Incorrect UserId");
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

        public bool UseClassicHttpWebRequest { get; set; }

        public bool UseTaskFactoryAsyncInsteadOfHttpClient { get; set; }

        public bool EnableTelemetry { get; set; } = true;

        public  int MaximumMessagesCacheSize { get; set; }

        public bool DedupOnSubscribe { get; set; }

        public bool SuppressLeaveEvents { get; set; }

        public bool UseRandomInitializationVector { get; set; }
        public int FileMessagePublishRetryLimit { get; set; }

        [Obsolete("PNConfiguration(string uuid) is deprecated, please use PNConfiguration(UserId userId) instead.")]
        public PNConfiguration(string uuid)
        {
            if (string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(uuid.Trim()))
            {
                throw new ArgumentException("Missing or Incorrect uuid value");
            }
            usingDeprecatedUuid = true;
            ConstructorInit(uuid, null);
        }

        public PNConfiguration(UserId userId)
        {
            if (userId == null || string.IsNullOrEmpty(userId.ToString()))
            {
                throw new ArgumentException("Missing or Incorrect UserId");
            }
            usingDeprecatedUuid = false;
            ConstructorInit(null, userId);
        }

        private void ConstructorInit(string currentUuid, UserId currentUserId) 
        {
            this.Origin = "ps.pndsn.com";
            this.presenceHeartbeatTimeout = 300;
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
            this.DedupOnSubscribe = false;
            this.MaximumMessagesCacheSize = 100;
            this.SuppressLeaveEvents = false;
            this.UseRandomInitializationVector = true;
            this.FileMessagePublishRetryLimit = 5;
            if (!string.IsNullOrEmpty(currentUuid))
            {
                _uuid = currentUuid;
                _userId = new UserId(currentUuid);
            }
            else if (currentUserId != null && !string.IsNullOrEmpty(currentUserId.ToString()))
            {
                _userId = currentUserId;
                _uuid = currentUserId.ToString();
            }
        }
        public PNConfiguration SetPresenceTimeoutWithCustomInterval(int timeout, int interval)
        {
            this.presenceHeartbeatTimeout = timeout;
            this.presenceHeartbeatInterval = interval;

            return this;
        }

    }
}
