// snippet.using
using PubnubApi;

// snippet.end

public class EntitiesSample
{
    private static Pubnub pubnub;
    private static PNConfiguration config;

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

    public static async Task GetAllUuidMetadataBasicUsage()
    {
        // snippet.get_all_uuid_metadata_basic_usage
        try
        {
            PNResult<PNGetAllUuidMetadataResult> getAllUuidMetadataResponse = await pubnub.GetAllUuidMetadata()
                .IncludeCustom(true)
                .IncludeCount(true)
                .ExecuteAsync();
            
            PNGetAllUuidMetadataResult getAllUuidMetadataResult = getAllUuidMetadataResponse.Result;
            PNStatus status = getAllUuidMetadataResponse.Status;

            if (!status.Error && getAllUuidMetadataResult != null)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getAllUuidMetadataResult));
            }
            else
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request cannot be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetUuidMetadataBasicUsage()
    {
        // snippet.get_uuid_metadata_basic_usage
        // Get Metadata for UUID set in the pubnub instance
        PNResult<PNGetUuidMetadataResult> getUuidSetMetadataResponse = await pubnub.GetUuidMetadata()
            .ExecuteAsync();
        PNGetUuidMetadataResult getUuidSetMetadataResult = getUuidSetMetadataResponse.Result;
        PNStatus uuidSetStatus = getUuidSetMetadataResponse.Status;

        // Get Metadata for a specific UUID
        PNResult<PNGetUuidMetadataResult> getSpecificUuidMetadataResponse = await pubnub.GetUuidMetadata()
            .Uuid("my-uuid")
            .ExecuteAsync();
        PNGetUuidMetadataResult getSpecificUuidMetadataResult = getSpecificUuidMetadataResponse.Result;
        PNStatus specificUuidStatus = getSpecificUuidMetadataResponse.Status;
        // snippet.end
    }

    public static async Task SetUuidMetadataBasicUsage()
    {
        // snippet.set_uuid_metadata_basic_usage
        // Set Metadata for UUID set in the pubnub instance
        PNResult<PNSetUuidMetadataResult> setUuidMetadataResponse = await pubnub.SetUuidMetadata()
            .Uuid(config.Uuid)
            .Name("John Doe")
            .Email("john.doe@user.com")
            .ExecuteAsync();
        PNSetUuidMetadataResult setUuidMetadataResult = setUuidMetadataResponse.Result;
        PNStatus status = setUuidMetadataResponse.Status;
        // snippet.end
    }

    public static async Task RemoveUuidMetadataBasicUsage()
    {
        // snippet.remove_uuid_metadata_basic_usage
        // Remove Metadata for UUID set in the pubnub instance
        PNResult<PNRemoveUuidMetadataResult> removeUuidMetadataResponse = await pubnub.RemoveUuidMetadata()
            .ExecuteAsync();
        PNRemoveUuidMetadataResult removeUuidMetadataResult = removeUuidMetadataResponse.Result;
        PNStatus status = removeUuidMetadataResponse.Status;
        // snippet.end
    }

    public static async Task GetAllChannelMetadataBasicUsage()
    {
        // snippet.get_all_channel_metadata_basic_usage
        PNResult<PNGetAllChannelMetadataResult> getAllChannelMetadataResponse = await pubnub.GetAllChannelMetadata()
            .IncludeCount(true)
            .IncludeCustom(true)
            .ExecuteAsync();

        PNGetAllChannelMetadataResult getAllChannelMetadataResult = getAllChannelMetadataResponse.Result;
        PNStatus status2 = getAllChannelMetadataResponse.Status;
        // snippet.end
    }

    public static async Task GetChannelMetadataBasicUsage()
    {
        // snippet.get_channel_metadata_basic_usage
        // Get Metadata for a specific channel
        PNResult<PNGetChannelMetadataResult> getChannelMetadataResponse = await pubnub.GetChannelMetadata()
            .Channel("my-channel")
            .IncludeCustom(true)
            .ExecuteAsync();

        PNGetChannelMetadataResult getChannelMetadataResult = getChannelMetadataResponse.Result;
        PNStatus status = getChannelMetadataResponse.Status;
        // snippet.end
    }

    public static async Task SetChannelMetadataBasicUsage()
    {
        // snippet.set_channel_metadata_basic_usage
        // Set Metadata for a specific channel
        PNResult<PNSetChannelMetadataResult> setChannelMetadataResponse = await pubnub.SetChannelMetadata()
            .Channel("my-channel")
            .Name("John Doe")
            .Description("sample description")
            .Custom(new Dictionary<string, object>() { { "color", "blue" } })
            .IncludeCustom(true)
            .ExecuteAsync();

        PNSetChannelMetadataResult setChannelMetadataResult = setChannelMetadataResponse.Result;
        PNStatus status = setChannelMetadataResponse.Status;
        // snippet.end
    }

    public static async Task SetChannelMetadataIterativeUpdate()
    {
        // snippet.set_channel_metadata_iterative_update
        PNConfiguration config = new PNConfiguration(new UserId("example"))
        {
            PublishKey = "demo",
            SubscribeKey = "demo",
        };
        Pubnub pubnub = new Pubnub(config);
        string channel = "team.red";
        string name = "Red Team";
        string description = "The channel for Red team.";
        var customField = new Dictionary<string, object>()
        {
            { "visible", "team" },
        };
        PNResult<PNSetChannelMetadataResult> setChannelMetadataResponse = await pubnub.SetChannelMetadata()
            .Channel(channel)
            .Name(name)
            .Description(description)
            .Custom(customField)
            .ExecuteAsync();
        Console.WriteLine("The channel has been created with name and description.\n");

        // Fetch current object with custom fields
        PNResult<PNGetChannelMetadataResult> currentObjectResponse = await pubnub.GetChannelMetadata()
            .Channel(channel)
            .IncludeCustom(true)
            .ExecuteAsync();
        var currentObject = currentObjectResponse.Result;

        // Initialize the custom field dictionary
        Dictionary<string, object> custom = currentObject?.Custom ?? new Dictionary<string, object>();


        // Add or update the field
        custom["edit"] = "admin";

        // Writing the updated object back to the server
        try
        {
            setChannelMetadataResponse = await pubnub.SetChannelMetadata()
                .Channel(channel)
                .Custom(custom)
                .Name(currentObject?.Name)
                .Description(currentObject?.Description)
                .ExecuteAsync();
            Console.WriteLine($"Object has been updated.\n {setChannelMetadataResponse.Result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        // snippet.end
    }

    public static async Task RemoveChannelMetadataBasicUsage()
    {
        // snippet.remove_channel_metadata_basic_usage
        // Delete Metadata for a specific channel
        PNResult<PNRemoveChannelMetadataResult> removeChannelMetadataResponse = await pubnub.RemoveChannelMetadata()
            .Channel("mychannel")
            .ExecuteAsync();

        PNRemoveChannelMetadataResult removeChannelMetadataResult = removeChannelMetadataResponse.Result;
        PNStatus status = removeChannelMetadataResponse.Status;
        // snippet.end
    }

    public static async Task GetMembershipsBasicUsage()
    {
        // snippet.get_memberships_basic_usage
        PNResult<PNMembershipsResult> getMembershipsResponse = await pubnub.GetMemberships()
            .Uuid("my-uuid")
            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
            .IncludeCount(true)
            .Page(new PNPageObject() { Next = "", Prev = "" })
            .ExecuteAsync();

        PNMembershipsResult getMembeshipsResult = getMembershipsResponse.Result;
        PNStatus status = getMembershipsResponse.Status;
        // snippet.end
    }

    public static async Task SetMembershipsBasicUsage()
    {
        // snippet.set_memberships_basic_usage
        List<PNMembership> setMembershipChannelMetadataIdList =
        [
            new PNMembership()
                { Channel = "my-channel", Custom = new Dictionary<string, object>() { { "item", "book" } } }
        ];

        PNResult<PNMembershipsResult> setMembershipsResponse = await pubnub.SetMemberships()
            .Uuid("my-uuid")
            .Channels(setMembershipChannelMetadataIdList)
            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
            .IncludeCount(true)
            .ExecuteAsync();

        PNMembershipsResult setMembershipsResult = setMembershipsResponse.Result;
        PNStatus status = setMembershipsResponse.Status;
        // snippet.end
    }

    public static async Task RemoveMembershipsBasicUsage()
    {
        // snippet.remove_memberships_basic_usage
        List<string> removeMembershipList =
        [
            "my-channel",
            "your-channel"
        ];

        PNResult<PNMembershipsResult> removeMembershipsResponse = await pubnub.RemoveMemberships()
            .Uuid("uuid")
            .Channels(removeMembershipList)
            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
            .IncludeCount(true)
            .ExecuteAsync();

        PNMembershipsResult removeMembershipsResult = removeMembershipsResponse.Result;
        PNStatus status2 = removeMembershipsResponse.Status;
        // snippet.end
    }

    public static async Task ManageMembershipsBasicUsage()
    {
        // snippet.manage_memberships_basic_usage
        List<PNMembership> setMembrshipList =
        [
            new PNMembership() { Channel = "ch1", Custom = new Dictionary<string, object>() { { "say", "hello" } } },
            new PNMembership() { Channel = "ch2", Custom = new Dictionary<string, object>() { { "say", "world" } } },
            new PNMembership() { Channel = "ch3", Custom = new Dictionary<string, object>() { { "say", "bye" } } }
        ];

        List<string> removeMembrshipList =
        [
            "ch4"
        ];

        PNResult<PNMembershipsResult> manageMmbrshipsResponse = await pubnub.ManageMemberships()
            .Uuid("my-uuid")
            .Set(setMembrshipList)
            .Remove(removeMembrshipList)
            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
            .IncludeCount(true)
            .Page(new PNPageObject() { Next = "", Prev = "" })
            .Sort(new List<string>() { "channel.id:asc" })
            .ExecuteAsync();

        PNMembershipsResult manageMmbrshipsResult = manageMmbrshipsResponse.Result;
        PNStatus status = manageMmbrshipsResponse.Status;
        // snippet.end
    }

    public static async Task GetChannelMembersBasicUsage()
    {
        // snippet.get_channel_members_basic_usage
        // Get Members (uuids) for a specific channel
        PNResult<PNChannelMembersResult> getChannelMembersResponse = await pubnub.GetChannelMembers()
            .Channel("my-channel")
            .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
            .IncludeCount(true)
            .ExecuteAsync();

        PNChannelMembersResult getChannelMembersResult = getChannelMembersResponse.Result;
        PNStatus status2 = getChannelMembersResponse.Status;
        // snippet.end
    }

    public static async Task SetChannelMembersBasicUsage()
    {
        // snippet.set_channel_members_basic_usage
        // Add Members (UUID) for a specific channel
        List<PNChannelMember> setMemberChannelList =
        [
            new PNChannelMember()
                { Uuid = "my-uuid", Custom = new Dictionary<string, object>() { { "planet", "earth" } } }
        ];
        
        PNResult<PNChannelMembersResult> setChannelMembersResponse = await pubnub.SetChannelMembers()
            .Channel("my-channel")
            .Uuids(setMemberChannelList)
            .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
            .IncludeCount(true)
            .ExecuteAsync();

        PNChannelMembersResult setChannelMembersResult = setChannelMembersResponse.Result;
        PNStatus status2 = setChannelMembersResponse.Status;
        // snippet.end
    }

    public static async Task RemoveChannelMembersBasicUsage()
    {
        // snippet.remove_channel_members_basic_usage
        // Remove Members (UUID) for a specific channel
        List<string> removeChannelMemberList =
        [
            "my-uuid",
            "your-uuid"
        ];

        PNResult<PNChannelMembersResult> removeChannelMembersResponse = await pubnub.RemoveChannelMembers()
            .Channel("my-channel")
            .Uuids(removeChannelMemberList)
            .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
            .IncludeCount(true)
            .ExecuteAsync();

        PNChannelMembersResult removeChannelMembersResult = removeChannelMembersResponse.Result;
        PNStatus status = removeChannelMembersResponse.Status;
        // snippet.end
    }
} 