// snippet.using
using PubnubApi;

// snippet.end

public class MessageActionsSample
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
    
    public static void AddMessageActionBasicUsage()
    {
        // snippet.add_message_action_basic_usage
        try
        {
            pubnub.AddMessageAction()
                .Channel("my_channel")
                .MessageTimetoken(5610547826969050) // Replace with actual message timetoken
                .Action(new PNMessageAction { Type = "reaction", Value = "smiley_face" })
                .Execute(new PNAddMessageActionResultExt((result, status) =>
                {
                    if (!status.Error && result != null)
                    {
                        Console.WriteLine("Message action added successfully.");
                    }
                    else
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request cannot be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static void RemoveMessageActionBasicUsage()
    {
        // snippet.remove_message_action_basic_usage
        pubnub.RemoveMessageAction()
            .Channel("my_channel")
            .MessageTimetoken(15701761818730000)
            .ActionTimetoken(15701775691010000)
            .Uuid("mytestuuid")
            .Execute(new PNRemoveMessageActionResultExt((result, status) =>
            {
                //empty result of type PNRemoveMessageActionResult.
            }));
        // snippet.end
    }

    public static void GetMessageActionsBasicUsage()
    {
        // snippet.get_message_actions_basic_usage
        pubnub.GetMessageActions()
            .Channel("my_channel")
            .Execute(new PNGetMessageActionsResultExt((result, status) =>
            {
                //result is of type PNGetMessageActionsResult.
            }));
        // snippet.end
    }
} 