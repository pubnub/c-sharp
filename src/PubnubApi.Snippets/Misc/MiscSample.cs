// snippet.using
using PubnubApi;

// snippet.end

public class MiscSample
{
    private static Pubnub pubnub;
    
    static void Init()
    {
        // snippet.init
        // Configuration
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo",
            PublishKey = "demo",
            Secure = true
        };

        // Initialize PubNub
        Pubnub pubnub = new Pubnub(pnConfiguration);
        
        // snippet.end
    }
    
    public static void DestroyBasicUsage()
    {
        // snippet.destroy_basic_usage
        // Destroy PubNub instance to clean up resources
        pubnub.Destroy();

        Console.WriteLine("PubNub instance destroyed successfully.");
        // snippet.end
    }

    public static void EncryptBasicUsage()
    {
        // snippet.encrypt_basic_usage
        string stringToEncrypt = "hello world";
        var crypto = PubnubApi.Security.Crypto.CryptoModule.CreateAesCbcCryptor("test");
        crypto.Encrypt(stringToEncrypt);
        // snippet.end
    }

    public static void EncryptFileBasicUsage()
    {
        // snippet.encrypt_file_basic_usage
        string source_file = "cat_picture.jpg"; // checks bin folder if no path is provided
        string destination_file = "destination_cat_pic.jpg"; // checks bin folder if no path is provided
        var crypto = PubnubApi.Security.Crypto.CryptoModule.CreateAesCbcCryptor("test");
        crypto.EncryptFile(source_file, destination_file);
        // snippet.end
    }

    public static void EncryptFileBytesBasicUsage()
    {
        // snippet.encrypt_file_bytes_basic_usage
        byte[] sourceBytes = System.IO.File.ReadAllBytes("cat_picture.jpg"); // checks bin folder if no path is provided
        var crypto = PubnubApi.Security.Crypto.CryptoModule.CreateAesCbcCryptor("test");
        byte[] outputBytes = crypto.Encrypt(sourceBytes);
        System.IO.File.WriteAllBytes("destination_cat_pic.jpg", outputBytes); // checks bin folder if no path is provided
        // snippet.end
    }

    public static void DecryptBasicUsage()
    {
        // snippet.decrypt_basic_usage
        var crypto = PubnubApi.Security.Crypto.CryptoModule.CreateAesCbcCryptor("test");
        crypto.Decrypt("encryptedString");
        // snippet.end
    }

    public static void DecryptFileBasicUsage()
    {
        // snippet.decrypt_file_basic_usage
        string source_file = "encrypted_cat_pic.jpg"; // checks bin folder if no path is provided
        string destination_file = "cat_pic_original.jpg"; // checks bin folder if no path is provided
        var crypto = PubnubApi.Security.Crypto.CryptoModule.CreateAesCbcCryptor("test");
        crypto.DecryptFile(source_file, destination_file);
        // snippet.end
    }

    public static void DecryptFileBytesBasicUsage()
    {
        // snippet.decrypt_file_bytes_basic_usage
        byte[] sourceBytes = System.IO.File.ReadAllBytes("encrypted_cat_pic.jpg"); // checks bin folder if no path is provided
        var crypto = PubnubApi.Security.Crypto.CryptoModule.CreateAesCbcCryptor("test");
        byte[] outputBytes = crypto.Decrypt(sourceBytes);
        System.IO.File.WriteAllBytes("cat_pic_original.jpg", outputBytes); // checks bin folder if no path is provided
        // snippet.end
    }

    public static void DisconnectBasicUsage()
    {
        // snippet.disconnect_basic_usage
        pubnub.Disconnect<string>();
        // snippet.end
    }

    public static void GetSubscribedChannelGroupsBasicUsage()
    {
        // snippet.get_subscribed_channel_groups_basic_usage
        List<string> groups = pubnub.GetSubscribedChannelGroups();
        // snippet.end
    }

    public static void GetSubscribedChannelsBasicUsage()
    {
        // snippet.get_subscribed_channels_basic_usage
        List<string> channels = pubnub.GetSubscribedChannels();
        // snippet.end
    }

    public static void ReconnectBasicUsage()
    {
        // snippet.reconnect_basic_usage
        pubnub.Reconnect<string>();
        // snippet.end
    }

    public static void TimeBasicUsage()
    {
        // snippet.time_basic_usage
        pubnub.Time()
            .Execute(new PNTimeResultExt(
                (result, status) => {
                    // handle time result.
                }
            ));
        // snippet.end
    }
} 