using PubnubApi.Security.Crypto;
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
        private UserId userId;
        private bool uuidSetFromConstructor;
        private PNReconnectionPolicy reconnectionPolicy;

        internal void ResetUuidSetFromConstructor()
        {
            uuidSetFromConstructor = false;
        }
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

        [Obsolete("CipherKey is deprecated, please use CryptoModule instead.", false)]
        public string CipherKey { get; set; }

        [Obsolete("UseRandomInitializationVector is deprecated, please use CryptoModule instead.", false)]
        public bool UseRandomInitializationVector { get; set; }
        public CryptoModule CryptoModule { get; set; }

        public string AuthKey { get; set; }

        [Obsolete("Uuid is deprecated, please use UserId instead.")]
        public string Uuid
        {
            get
            {
                return userId.ToString();
            }
            set
            {
                if (userId != null && !uuidSetFromConstructor)
                {
                    throw new ArgumentException("Either UserId or Uuid can be used. Not both.");
                }

                if (value != null && value.Trim().Length > 0)
                {
                    userId = new UserId(value);
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
                return userId;
            }
            set
            {
                if (uuidSetFromConstructor)
                {
                    throw new ArgumentException("Either UserId or Uuid can be used. Not both.");
                }

                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    uuidSetFromConstructor = false;
                    userId = value;
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

        public int SubscribeTimeout { get; set; } = 310;

        public int NonSubscribeRequestTimeout { get; set; } = 15;

        public PNHeartbeatNotificationOption HeartbeatNotificationOption { get; set; }

        public string FilterExpression { get; set; }

        public bool IncludeInstanceIdentifier { get; set; }

        public bool IncludeRequestIdentifier { get; set; }

        public PNReconnectionPolicy ReconnectionPolicy {
            get => reconnectionPolicy;
            set {
                reconnectionPolicy = value;
                setDefaultRetryConfigurationFromPolicy(value);
            }
        }

        public RetryConfiguration RetryConfiguration { get; set; }

        public int RequestMessageCountThreshold { get; set; } = 100;

        public bool UseClassicHttpWebRequest { get; set; }

        public bool UseTaskFactoryAsyncInsteadOfHttpClient { get; set; }

        public bool EnableTelemetry { get; set; } = true;

        public  int MaximumMessagesCacheSize { get; set; }

        public bool DedupOnSubscribe { get; set; }

        public bool SuppressLeaveEvents { get; set; }

        public bool MaintainPresenceState { get; set; } = true;

        public bool EnableEventEngine { get; set; } = true;

        public int FileMessagePublishRetryLimit { get; set; }

        [Obsolete("PNConfiguration(string uuid) is deprecated, please use PNConfiguration(UserId userId) instead.")]
        public PNConfiguration(string uuid)
        {
            if (string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(uuid.Trim()))
            {
                throw new ArgumentException("Missing or Incorrect uuid value");
            }
            uuidSetFromConstructor = true;
            ConstructorInit(new UserId(uuid));
        }

        public PNConfiguration(UserId userId)
        {
            if (userId == null || string.IsNullOrEmpty(userId.ToString()))
            {
                throw new ArgumentException("Missing or Incorrect UserId");
            }
            uuidSetFromConstructor = false;
            ConstructorInit(userId);
        }

        private void ConstructorInit(UserId currentUserId) 
        {
            Origin = "ps.pndsn.com";
            presenceHeartbeatTimeout = 300;
            NonSubscribeRequestTimeout = 10;
            SubscribeTimeout = 310;
            LogVerbosity = PNLogVerbosity.NONE;
            CipherKey = "";
            PublishKey = "";
            SubscribeKey = "";
            SecretKey = "";
            Secure = true;
            ReconnectionPolicy = PNReconnectionPolicy.NONE;
            HeartbeatNotificationOption = PNHeartbeatNotificationOption.Failures;
            IncludeRequestIdentifier = true;
            IncludeInstanceIdentifier = false;
            DedupOnSubscribe = false;
            MaximumMessagesCacheSize = 100;
            SuppressLeaveEvents = false;
            UseRandomInitializationVector = true;
            FileMessagePublishRetryLimit = 5;
            userId = currentUserId;
            EnableEventEngine = false;
        }

        private void setDefaultRetryConfigurationFromPolicy(PNReconnectionPolicy policy)
        {
            switch (policy) 
            {
                case PNReconnectionPolicy.LINEAR:
                    RetryConfiguration = RetryConfiguration.Linear(2, 10);
                    break;
                case PNReconnectionPolicy.EXPONENTIAL:
                    RetryConfiguration = RetryConfiguration.Exponential(2, 150, 6);
                    break;
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
