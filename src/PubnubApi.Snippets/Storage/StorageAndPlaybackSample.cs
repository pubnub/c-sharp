// snippet.using
using PubnubApi;

// snippet.end

public class StorageAndPlaybackSample
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

    public static async Task FetchHistoryBasicUsage()
    {
        // snippet.fetch_history_basic_usage
        try
        {
            // Fetch historical messages
            PNResult<PNFetchHistoryResult> fetchHistoryResponse = await pubnub.FetchHistory()
                .Channels(new string[] { "my_channel" })
                .IncludeMeta(true)
                .IncludeCustomMessageType(true)
                .MaximumPerChannel(25)
                .ExecuteAsync();
            
            PNFetchHistoryResult fetchHistoryResult = fetchHistoryResponse.Result;
            PNStatus status = fetchHistoryResponse.Status;

            if (!status.Error && fetchHistoryResult != null)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistoryResult));
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

    public static void FetchHistorySynchronous()
    {
        // snippet.fetch_history_synchronous
        pubnub.FetchHistory()
            .Channels(new string[] { "my_channel" })
            .IncludeMeta(true)
            .MaximumPerChannel(25)
            .Execute(new PNFetchHistoryResultExt((result, status) =>
            {

            }));
        // snippet.end
    }

    public static async Task DeleteMessagesBasicUsage()
    {
        // snippet.delete_messages_basic_usage
        PNResult<PNDeleteMessageResult> delMsgResponse = await pubnub.DeleteMessages()
            .Channel("history_channel")
            .Start(15088506076921021)
            .End(15088532035597390)
            .ExecuteAsync();

        PNDeleteMessageResult delMsgResult = delMsgResponse.Result;
        PNStatus status = delMsgResponse.Status;

        if (status != null && status.Error)
        {
            //Check for any error
            Console.WriteLine(status.ErrorData.Information);
        }
        else if (delMsgResult != null)
        {
            //Expect empty object
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(delMsgResult));
        }
        // snippet.end
    }

    public static void DeleteMessagesSynchronous()
    {
        // snippet.delete_messages_synchronous
        pubnub.DeleteMessages()
            .Channel("history_channel")
            .Start(15088506076921021)
            .End(15088532035597390)
            .Execute(new PNDeleteMessageResultExt(
                (result, status) => {
                    if (status != null && status.Error) {
                        //Check for any error
                        Console.WriteLine(status.ErrorData.Information);
                    }
                    else if (result != null) {
                        //Expect empty object
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                    }
                }
            ));
        // snippet.end
    }

    public static async Task DeleteSpecificMessage()
    {
        // snippet.delete_specific_message
        PNResult<PNDeleteMessageResult> delMsgResponse = await pubnub.DeleteMessages()
            .Channel("history_channel")
            .Start(15526611838554309)
            .End(15526611838554310)
            .ExecuteAsync();

        PNDeleteMessageResult delMsgResult = delMsgResponse.Result;
        PNStatus status = delMsgResponse.Status;

        if (status != null && status.Error)
        {
            //Check for any error
            Console.WriteLine(status.ErrorData.Information);
        }
        else if (delMsgResult != null)
        {
            //Expect empty object
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(delMsgResult));
        }
        // snippet.end
    }

    public static async Task MessageCountsBasicUsage()
    {
        // snippet.message_counts_basic_usage
        PNResult<PNMessageCountResult> msgCountResponse = await pubnub.MessageCounts()
            .Channels(new string[] { "message_count_channel" })
            .ChannelsTimetoken(new long[] { 15088506076921021 })
            .ExecuteAsync();

        PNMessageCountResult msgCountResult = msgCountResponse.Result;
        PNStatus status = msgCountResponse.Status;

        if (status != null && status.Error)
        {
            //Check for any error
            Console.WriteLine(status.ErrorData.Information);
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(msgCountResult));
        }
        // snippet.end
    }

    public static void MessageCountsSynchronous()
    {
        // snippet.message_counts_synchronous
        pubnub.MessageCounts()
            .Channels(new string[] { "message_count_channel" })
            .ChannelsTimetoken(new long[] { 15088506076921021 })
            .Execute(new PNMessageCountResultExt(
            (result, status) => {
                if (status != null && status.Error)
                {
                    //Check for any error
                    Console.WriteLine(status.ErrorData.Information);
                }
                else
                {
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                }
            }));
        // snippet.end
    }

    public static async Task MessageCountsMultipleChannels()
    {
        // snippet.message_counts_multiple_channels
        PNResult<PNMessageCountResult> msgCountResponse = await pubnub.MessageCounts()
            .Channels(new string[] { "message_count_channel", "message_count_channel2" })
            .ChannelsTimetoken(new long[] { 15088506076921021, 15088506076921131 })
            .ExecuteAsync();

        PNMessageCountResult msgCountResult = msgCountResponse.Result;
        PNStatus status = msgCountResponse.Status;

        if (status != null && status.Error)
        {
            //Check for any error
            Console.WriteLine(status.ErrorData.Information);
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(msgCountResult));
        }
        // snippet.end
    }

    public static async Task HistoryBasicUsage()
    {
        // snippet.history_basic_usage
        PNResult<PNHistoryResult> historyResponse = await pubnub.History()
            .Channel("history_channel") // where to fetch history from
            .Count(100) // how many items to fetch
            .ExecuteAsync();
        // snippet.end
    }

    public static void HistorySynchronous()
    {
        // snippet.history_synchronous
        pubnub.History()
            .Channel("history_channel") // where to fetch history from
            .Count(100) // how many items to fetch
            .Execute(new PNHistoryResultExt(
                (result, status) => {
                }
            ));
        // snippet.end
    }

    public static async Task HistoryReverse()
    {
        // snippet.history_reverse
        PNResult<PNHistoryResult> historyResponse = await pubnub.History()
            .Channel("my_channel") // where to fetch history from
            .Count(3) // how many items to fetch
            .Reverse(true) // should go in reverse?
            .ExecuteAsync();
        // snippet.end
    }

    public static async Task HistoryStartTimetoken()
    {
        // snippet.history_start_timetoken
        PNResult<PNHistoryResult> historyResponse = await pubnub.History()
            .Channel("my_channel") // where to fetch history from
            .Start(13847168620721752L) // first timestamp
            .Reverse(true) // should go in reverse?
            .ExecuteAsync();
        // snippet.end
    }

    public static async Task HistoryStartEndTimetoken()
    {
        // snippet.history_start_end_timetoken
        PNResult<PNHistoryResult> historyResponse = await pubnub.History()
            .Channel("my_channel") // where to fetch history from
            .Count(100) // how many items to fetch
            .Start(-1) // first timestamp
            .End(13847168819178600L) // last timestamp
            .Reverse(true) // should go in reverse?
            .ExecuteAsync();
        // snippet.end
    }

    public static async Task HistoryIncludeTimetoken()
    {
        // snippet.history_include_timetoken
        PNResult<PNHistoryResult> historyResponse = await pubnub.History()
            .Channel("history_channel") // where to fetch history from
            .Count(100) // how many items to fetch
            .IncludeTimetoken(true) // include timetoken with each entry
            .ExecuteAsync();
        // snippet.end
    }

    // snippet.history_paging_example
    public class PubnubRecursiveHistoryFetcher {
        private static Pubnub pubnub;

        public abstract class CallbackSkeleton {
            public abstract void HandleResponse(PNHistoryResult result);
        }

        public PubnubRecursiveHistoryFetcher() {
            // NOTICE: for demo/demo pub/sub keys Message Persistence is disabled,
            // so use your pub/sub keys instead
            PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
            pnConfiguration.SubscribeKey = "demo";
            pubnub = new Pubnub(pnConfiguration);
        }

        static public void Main() {
            PubnubRecursiveHistoryFetcher fetcher = new PubnubRecursiveHistoryFetcher();
            GetAllMessages(new DemoCallbackSkeleton());
        }

        public static void GetAllMessages(CallbackSkeleton callback) {
            GetAllMessages(-1L, callback);
        }

        public static void GetAllMessages(long startTimestamp, CallbackSkeleton callback) {
            CountdownEvent latch = new CountdownEvent(1);

            pubnub.History()
                .Channel("history_channel") // where to fetch history from
                .Count(100) // how many items to fetch
                .Start(startTimestamp) // first timestamp
                .Execute(new DemoHistoryResult(callback));
        }

        public class DemoHistoryResult : PNCallback<PNHistoryResult> {
            CallbackSkeleton internalCallback;
            public DemoHistoryResult(CallbackSkeleton callback) {
                this.internalCallback = callback;
            }
            public override void OnResponse(PNHistoryResult result, PNStatus status) {
                if (!status.Error && result != null && result.Messages != null && result.Messages.Count > 0) {
                    Console.WriteLine(result.Messages.Count);
                    Console.WriteLine("start:" + result.StartTimeToken.ToString());
                    Console.WriteLine("end:" + result.EndTimeToken.ToString());

                    internalCallback.HandleResponse(result);
                    GetAllMessages(result.EndTimeToken, this.internalCallback);
                }
            }
        };

        public class DemoCallbackSkeleton : CallbackSkeleton {
            public override void HandleResponse(PNHistoryResult result) {
                //Handle the result
            }
        }
    }
    // snippet.end
} 