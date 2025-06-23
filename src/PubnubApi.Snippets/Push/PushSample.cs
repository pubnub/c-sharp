// snippet.using
using PubnubApi;

// snippet.end

public class PushSample
{
    private static Pubnub pubnub;

    static void Init()
    {
        // snippet.pubnub_init
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
    
    // snippet.add_device_to_channel_basic_usage
    public class PushNotificationCallback : PNCallback<PNPushAddChannelResult>
    {
        public override void OnResponse(PNPushAddChannelResult result, PNStatus status)
        {
            if (!status.Error && result != null)
            {
                Console.WriteLine("Push notifications added to channels successfully.");
            }
            else
            {
                Console.WriteLine("Failed to add push notifications: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }
    
    public static void AddDeviceToChannelBasicUsage()
    {
        // Configuration
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo",
            PublishKey = "demo",
            Secure = true
        };

        // Initialize PubNub
        Pubnub pubnub = new Pubnub(pnConfiguration);
        
        try
        {
            // For FCM
            pubnub.AddPushNotificationsOnChannels()
                .PushType(PNPushType.FCM)
                .Channels(new string[] { "ch1", "ch2", "ch3" })
                .DeviceId("googleDevice")
                .Execute(new PushNotificationCallback());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FCM operation failed due to error: {ex.Message}");
        }

        try
        {
            // For APNS2
            pubnub.AddPushNotificationsOnChannels()
                .PushType(PNPushType.APNS2)
                .Channels(new string[] { "ch1", "ch2", "ch3" })
                .DeviceId("appleDevice")
                .Topic("myapptopic")
                .Environment(PushEnvironment.Development)
                .Execute(new PushNotificationCallback());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"APNS2 operation failed due to error: {ex.Message}");
        }
    }
    // snippet.end

    public static void ListChannelsForDeviceBasicUsage()
    {
        // snippet.list_channels_for_device_basic_usage
        // for FCM/GCM
        pubnub.AuditPushChannelProvisions()
            .DeviceId("googleDevice")
            .PushType(PNPushType.FCM)
            .Execute(new PNPushListProvisionsResultExt((r, s) =>
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
            }));

        // for APNS2
        pubnub.AuditPushChannelProvisions()
            .DeviceId("appleDevice")
            .PushType(PNPushType.APNS2)
            .Topic("myapptopic")
            .Environment(PushEnvironment.Development)
            .Execute(new PNPushListProvisionsResultExt((r, s) =>
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
            }));
        // snippet.end
    }

    public static void RemoveDeviceFromChannelBasicUsage()
    {
        // snippet.remove_device_from_channel_basic_usage
        // for FCM/GCM
        pubnub.RemovePushNotificationsFromChannels()
            .DeviceId("googleDevice")
            .Channels(new string[] {
                "ch1",
                "ch2",
                "ch3"
            })
            .PushType(PNPushType.FCM)
            .Execute(new PNPushRemoveChannelResultExt((r, s) =>
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
            }));

        // for APNS2
        pubnub.RemovePushNotificationsFromChannels()
            .DeviceId("appleDevice")
            .Channels(new string[] {
                "ch1",
                "ch2",
                "ch3"
            })
            .PushType(PNPushType.APNS2)
            .Topic("myapptopic")
            .Environment(PushEnvironment.Development)
            .Execute(new PNPushRemoveChannelResultExt((r, s) =>
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
            }));
        // snippet.end
    }

    public static void RemoveAllPushNotificationsBasicUsage()
    {
        // snippet.remove_all_push_notifications_basic_usage
        // for FCM/GCM
        pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
            .DeviceId("googleDevice")
            .PushType(PNPushType.FCM)
            .Execute(new PNPushRemoveAllChannelsResultExt((r, s) => {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
            }));

        // for APNS2
        pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
            .DeviceId("appleDevice")
            .PushType(PNPushType.APNS2)
            .Topic("myapptopic")
            .Environment(PushEnvironment.Development)
            .Execute(new PNPushRemoveAllChannelsResultExt((r, s) => {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
            }));
        // snippet.end
    }
} 