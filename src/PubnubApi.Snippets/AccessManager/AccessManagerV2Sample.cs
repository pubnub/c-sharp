// snippet.using
using PubnubApi;

// snippet.end

public class AccessManagerV2Sample
{
    private static Pubnub pubnub;

    static void PubnubInit()
    {
        // snippet.pubnub_init
        //Create configuration
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo",
            PublishKey = "demo"
        };
        //Create a new PubNub instance
        Pubnub pubnub = new Pubnub(pnConfiguration);
        
        // snippet.end
    }

    static async Task Grant()
    {
        // snippet.grant
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Channels(new string[]{
                //channels to allow grant on
                "ch1",
                "ch2",
                "ch3"
            })
            .ChannelGroups(new string[] {
                // groups to allow grant on
                "cg1",
                "cg2",
                "cg3"
            })
            .AuthKeys(new string[] {
                // the keys we are provisioning
                "key1",
                "key2",
                "key3"
            })
            .Write(true) // allow those keys to write (false by default)
            .Manage(true) // allow those keys to manage channel groups (false by default)
            .Read(true) // allow keys to read the subscribe feed (false by default)
            .Delete(true) // allow those keys to delete the subscribe feed (false by default)
            .TTL(12337) // how long those keys will remain valid (0 for eternity)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static void GrantCallback()
    {
        // snippet.grant_callback
        pubnub.Grant()
            .Channels(new string[]{
                //channels to allow grant on
                "ch1",
                "ch2",
                "ch3"
            })
            .ChannelGroups(new string[] {
                // groups to allow grant on
                "cg1",
                "cg2",
                "cg3"
            })
            .AuthKeys(new string[] {
                // the keys we are provisioning
                "key1",
                "key2",
                "key3"
            })
            .Write(true) // allow those keys to write (false by default)
            .Manage(true) // allow those keys to manage channel groups (false by default)
            .Read(true) // allow keys to read the subscribe feed (false by default)
            .Delete(true) // allow those keys to delete the subscribe feed (false by default)
            .TTL(12337) // how long those keys will remain valid (0 for eternity)
            .Execute(new PNAccessManagerGrantResultExt(
                (result, status) => {
                    //PNAccessManagerGrantResult is a parsed and abstracted response from server
                }
            ));
        // snippet.end
    }
    
    static async Task GrantTTL()
    {
        // snippet.grant_ttl
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Channels(new string[] {
                "my_channel"
            })
            .Write(false)
            .Read(true)
            .Delete(false)
            .AuthKeys(new string[] {
                "my_ro_authkey"
            })
            .TTL(5)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static async Task GrantPresence()
    {
        // snippet.grant_presence
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Channels(new string[] {
                "my_channel-pnpres"
            })
            .Write(true)
            .Read(true)
            .Delete(true)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static async Task GrantChannelGroup()
    {
        // snippet.grant_group
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .ChannelGroups(new string[] {
                "cg1",
                "cg2",
                "cg3"
            })
            .AuthKeys(new string[] {
                "key1",
                "key2",
                "key3"
            })
            .Write(true)
            .Manage(true)
            .Read(true)
            .Delete(true)
            .TTL(12337)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static async Task GrantWithAuthKey()
    {
        // snippet.grant_auth_key
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Uuids(new string[] {
                "my_uuid"
            })
            .Get(true)
            .Update(true)
            .Delete(true)
            .AuthKeys(new string[] {
                "my_ro_authkey"
            })
            .TTL(1440)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static async Task GrantAppLevel()
    {
        // snippet.grant_app_level
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Write(true)
            .Read(true)
            .Delete(true)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static async Task GrantChannelLevel()
    {
        // snippet.grant_channel_level
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Channels(new string[] {
                "my_channel"
            })
            .Write(true)
            .Read(true)
            .Delete(true)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
    
    static async Task GrantUserLevel()
    {
        // snippet.grant_user_level
        PNResult<PNAccessManagerGrantResult> grantResponse = await pubnub.Grant()
            .Channels(new string[] {
                "my_channel"
            })
            .Write(true)
            .Read(true)
            .Delete(true)
            .AuthKeys(new string[]{
                "my_authkey"
            })
            .TTL(5)
            .ExecuteAsync();

        PNAccessManagerGrantResult grantResult = grantResponse.Result;
        PNStatus status = grantResponse.Status;
        //PNAccessManagerGrantResult is a parsed and abstracted response from server
        // snippet.end
    }
}