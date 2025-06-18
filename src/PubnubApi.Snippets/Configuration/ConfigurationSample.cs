// snippet.using
using PubnubApi;

// snippet.end

// snippet.using_crypto
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
// snippet.end

public class ConfigurationSample
{
    private static Pubnub pubnub;

    static void PubnubInit()
    {
        // snippet.init_config
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        // snippet.end
        
        // snippet.crypto
        // encrypts using 256-bit AES-CBC cipher (recommended)
        // decrypts data encrypted with the legacy and the 256-bit AES-CBC ciphers
        pnConfiguration.CryptoModule = new CryptoModule(new AesCbcCryptor("enigma"),
            new List<ICryptor> { new LegacyCryptor("enigma") });

        // encrypts with 128-bit cipher key entropy (legacy)
        // decrypts data encrypted with the legacy and the 256-bit AES-CBC ciphers
        pnConfiguration.CryptoModule = new CryptoModule(new LegacyCryptor("enigma"),
            new List<ICryptor> { new AesCbcCryptor("enigma") });
        // snippet.end
        
        // snippet.new_pubnub
        Pubnub pubnub = new Pubnub(pnConfiguration);
        
        // snippet.end
    }

    static void BasicUsage()
    {
        // snippet.basic_usage
        // Create a configuration instance for PubNub
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo", // Required
            PublishKey = "demo",   // Only required if publishing
            SecretKey = "SecretKey", // Only required for access operations
            Secure = true, // Enable SSL
            AuthKey = "authKey", // If Access Manager is utilized
            LogLevel = PubnubLogLevel.Debug, // Enable debugging
            SubscribeTimeout = 310, // Subscribe loop timeout in seconds
            NonSubscribeRequestTimeout = 300, // Non-subscribe request timeout
            FilterExpression = "such=wow", // PSV2 filter expression
            HeartbeatNotificationOption = PNHeartbeatNotificationOption.All, // Heartbeat notifications
            PresenceTimeout = 120, // Presence timeout
        };

        // Configure presence timeout with custom interval
        pnConfiguration.SetPresenceTimeoutWithCustomInterval(120, 59);

        // Encryption configuration (Optional)
        pnConfiguration.CryptoModule = new CryptoModule(
            new AesCbcCryptor("enigma"), 
            new List<ICryptor> { new LegacyCryptor("enigma") });

        // Initialize a new PubNub instance with the created confiiguration
        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
        
        // snippet.user_id
        pnConfiguration.UserId = new UserId("myUserId");
        // snippet.end
        
        // snippet.get_user_id
        UserId currentUserId = pubnub.GetCurrentUserId();
        // snippet.end
    }

    static void ChangeUserId()
    {
        // snippet.change_user_id
        //Setting the initial UserId
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        //Changing to a new UserId
        pnConfiguration.UserId = new UserId("myUserId");
        // snippet.end
    }
    
    static void SetAndGetAuthKey()
    {
        // snippet.set_auth_key
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        pnConfiguration.AuthKey = "authKey";
        // snippet.end
        
        // snippet.get_auth_key
        string sampleAuthKey = pnConfiguration.AuthKey;
        // snippet.end
    }

    static void FilterExpression()
    {
        // snippet.filter_expression
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        pnConfiguration.FilterExpression = "such=wow";
        // snippet.end
        
        // snippet.get_filter_expression
        string filterExpression = pnConfiguration.FilterExpression;
        // snippet.end
    }

    static void InitWithUUID()
    {
        // snippet.init_with_uuid
        // Initialize PubNub using the configuration
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo",
            PublishKey = "demo",
            Secure = true
        };

        // Create the PubNub instance with the configuration
        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
    }
    
    static void InitNonSecure()
    {
        // snippet.init_non_secure
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        pnConfiguration.PublishKey = "my_pubkey";
        pnConfiguration.SubscribeKey = "my_subkey";
        pnConfiguration.Secure = false;
        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
    }
    
    static void InitSecure()
    {
        // snippet.init_secure
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        pnConfiguration.PublishKey = "my_pubkey";
        pnConfiguration.SubscribeKey = "my_subkey";
        pnConfiguration.Secure = true;
        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
    }
    
    static void InitSecretKey()
    {
        // snippet.init_secret_key
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        pnConfiguration.PublishKey = "my_pubkey";
        pnConfiguration.SubscribeKey = "my_subkey";
        pnConfiguration.SecretKey = "my_secretkey";
        pnConfiguration.Secure = true;

        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
    }
    
    static void InitReadOnly()
    {
        // snippet.init_read_only
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
        pnConfiguration.SubscribeKey = "my_subkey";
        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
    }
}