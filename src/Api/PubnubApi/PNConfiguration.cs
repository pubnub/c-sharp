using PubnubApi.Security.Crypto;
using System;

namespace PubnubApi
{
	public class PNConfiguration
    {
        private int presenceHeartbeatTimeout;
        private int presenceHeartbeatInterval;
        private UserId userId;
        private bool uuidSetFromConstructor;
        private PNReconnectionPolicy reconnectionPolicy;
        private int subscribeTimeout;
        private CryptoModule cryptoModule;
        private string authKey;
        private string filterExpression;
        private string subscribeKey;
        private string publishKey;
        private string secretKey;
        private string cipherKey;

        internal void ResetUuidSetFromConstructor()
        {
            uuidSetFromConstructor = false;
        }
        public string Origin { get; set; }

        public int PresenceTimeout
        {
            get => presenceHeartbeatTimeout;
            set
            {
                presenceHeartbeatTimeout = value;
                SetPresenceTimeoutWithCustomInterval(presenceHeartbeatTimeout, (presenceHeartbeatTimeout / 2) - 1);
                Logger?.Debug($"PreseceHeartbeatTimeout set to {value}");
            }
        }

        public int PresenceInterval => presenceHeartbeatInterval;

        public bool Secure { get; set; }

        public string SubscribeKey
        {
            get => subscribeKey;
            set
            {
                subscribeKey = value;
                Logger?.Debug($"Subscribe Key set to {value}");
            }
        }

        public string PublishKey
        {
            get => publishKey;
            set
            {
                publishKey = value;
                Logger?.Debug($"Publish Key set to {value}");
            }
        }

        public string SecretKey
        {
            get => secretKey;
            set
            {
                secretKey = value;
                Logger?.Debug($"Secret Key value is set.");
            }
        }

        [Obsolete("CipherKey is deprecated, please use CryptoModule instead.", false)]
        public string CipherKey
        {
            get => cipherKey;
            set
            {
                cipherKey = value;
                Logger?.Debug($"CipherKey is set.");
            }
        }

        [Obsolete("UseRandomInitializationVector is deprecated, please use CryptoModule instead.", false)]
        public bool UseRandomInitializationVector { get; set; }
        public CryptoModule CryptoModule
        {
            get => cryptoModule;
            set
            {
                cryptoModule = value;
                Logger?.Debug($"CryptoModule initialised.");
            }
        }

        public PubnubLogModule Logger { get; internal set; }

        public string AuthKey
        {
            get => authKey;
            set
            {
                authKey = value;
                Logger?.Debug($"AuthKey value is set.");
            }
        }

        [Obsolete("Uuid is deprecated, please use UserId instead.")]
        public string Uuid
        {
            get => userId.ToString();
            set
            {
                if (userId != null && !uuidSetFromConstructor)
                {
                    throw new ArgumentException("Either UserId or Uuid can be used. Not both.");
                }

                if (value != null && value.Trim().Length > 0)
                {
                    userId = new UserId(value);
                    Logger?.Debug($"UserId set to {value}");
                }
                else
                {
                    throw new ArgumentException("Missing or Incorrect Uuid value");
                }
            }
        }

        public UserId UserId
        {
            get => userId;
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
                    Logger?.Debug($"UserId set to {value}");
                }
                else
                {
                    throw new ArgumentException("Missing or Incorrect UserId");
                }
            }
        }

        /// <summary>
        /// This property is obsolete. Use <see cref="PNConfiguration.LogLevel"/> instead.
        /// </summary>
        /// <remarks>
        /// LogVerbosity is deprecated. Use LogLevel to enable logging.
        /// LogLevel provides more granular control and supports different standard logging levels.
        /// To migrate, replace LogVerbosity = X with LogLevel = (LogLevel)X.
        /// </remarks>
        [Obsolete("LogVerbosity is deprecated. Use LogLevel to enable and configure logging.", false)]
        public PNLogVerbosity LogVerbosity { get; set; }

        public PubnubLogLevel LogLevel { get; set; }
        /// <summary>
        /// This property is deprecated. Use <see cref="Pubnub.SetLogger"/> method to configure custom logger.
        /// </summary>
        /// <remarks>
        /// PubnubLog is deprecated. Implement IPubnubLogger and Use SetLogger method to configure custom logger for improved flexibility and control.
        /// </remarks>
        [Obsolete("PubnubLog is deprecated. Use SetLogger method to configure custom logger after implementing IPubnubLogger interface.", false)]
        public IPubnubLog PubnubLog { get; set; }

        public Proxy Proxy { get; set; }

        public int SubscribeTimeout
        {
            get => subscribeTimeout <= 0 ? 310:subscribeTimeout;
            set
            {
                if (subscribeTimeout >= 0)
                {
                    subscribeTimeout = value;
                    Logger?.Debug($"Subscribe Timeout set to {value}");
                }
            }
        }

        public int NonSubscribeRequestTimeout { get; set; } = 15;

        public PNHeartbeatNotificationOption HeartbeatNotificationOption { get; set; }

        public string FilterExpression {
            get => filterExpression;
            set
            {
                filterExpression = value;
                Logger?.Debug($"FilterExpression set to {value}");
            }
        }

        public bool IncludeInstanceIdentifier { get; set; }

        public bool IncludeRequestIdentifier { get; set; }

        public PNReconnectionPolicy ReconnectionPolicy {
            get => reconnectionPolicy;
            set {
                reconnectionPolicy = value;
                setDefaultRetryConfigurationFromPolicy(value);
                Logger?.Debug($"ReconnectionPolicy set to {value}");
            }
        }

        public RetryConfiguration RetryConfiguration { get; set; }

        public int RequestMessageCountThreshold { get; set; } = 100;

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
            Logger = new PubnubLogModule(logLevel: PubnubLogLevel.None);
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
            NonSubscribeRequestTimeout = 15;
            SubscribeTimeout = 310;
            LogVerbosity = PNLogVerbosity.NONE;
            CipherKey = "";
            PublishKey = "";
            SubscribeKey = "";
            SecretKey = "";
            Secure = true;
            ReconnectionPolicy = PNReconnectionPolicy.EXPONENTIAL;
            HeartbeatNotificationOption = PNHeartbeatNotificationOption.Failures;
            IncludeRequestIdentifier = true;
            IncludeInstanceIdentifier = false;
            DedupOnSubscribe = false;
            MaximumMessagesCacheSize = 100;
            SuppressLeaveEvents = false;
            UseRandomInitializationVector = true;
            FileMessagePublishRetryLimit = 5;
            userId = currentUserId;
            LogLevel = PubnubLogLevel.None;
            EnableEventEngine = true;
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
            presenceHeartbeatTimeout = timeout;
            presenceHeartbeatInterval = interval;
            Logger?.Debug($"PresenceTimeoutInterval set to {interval}");
            return this;
        }

    }
}
