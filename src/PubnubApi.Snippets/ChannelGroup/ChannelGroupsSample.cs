// snippet.using
using PubnubApi;

// snippet.end

public class ChannelGroupsSample
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

    static async Task AddToGroup()
    {
        // snippet.add_to_group
        try
        {
            PNResult<PNChannelGroupsAddChannelResult> cgAddChResponse = await pubnub.AddChannelsToChannelGroup()
                .ChannelGroup("myChannelGroup")
                .Channels(new string[] { "channel1", "channel2", "channel3" })
                .ExecuteAsync();

            PNChannelGroupsAddChannelResult cgAddChResult = cgAddChResponse.Result;
            PNStatus cgAddChStatus = cgAddChResponse.Status;

            if (!cgAddChStatus.Error && cgAddChResult != null)
            {
                Console.WriteLine("Channels successfully added to the channel group.");
            }
            else
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(cgAddChStatus));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request cannot be executed due to error: {ex.Message}");
        }
        // snippet.end
    }
    
    static async Task ListFromGroup()
    {
        // snippet.list
        PNResult<PNChannelGroupsAllChannelsResult> cgListChResponse = await pubnub.ListChannelsForChannelGroup()
            .ChannelGroup("cg1")
            .ExecuteAsync();
        // snippet.end
    }
    
    static async Task RemoveFromGroup()
    {
        // snippet.remove
        PNResult<PNChannelGroupsRemoveChannelResult> rmChFromCgResponse = await pubnub.RemoveChannelsFromChannelGroup()
            .ChannelGroup("family")
            .Channels(new string[] {
                "son"
            })
            .ExecuteAsync();
        // snippet.end
    }
    
    static async Task DeleteGroup()
    {
        // snippet.delete
        PNResult<PNChannelGroupsDeleteGroupResult> delCgResponse = await pubnub.DeleteChannelGroup()
            .ChannelGroup("family")
            .ExecuteAsync();
        // snippet.end
    }
}