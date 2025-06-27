// snippet.using
using PubnubApi;

// snippet.end

public class PresenceSample
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

    public static async Task HereNowBasicUsage()
    {
        // snippet.here_now_basic_usage
        try
        {
            PNResult<PNHereNowResult> herenowResponse = await pubnub.HereNow()
                .Channels(new string[] { "coolChannel", "coolChannel2" })
                .IncludeUUIDs(true)
                .ExecuteAsync();

            PNHereNowResult herenowResult = herenowResponse.Result;
            PNStatus status = herenowResponse.Status;

            if (!status.Error && herenowResult != null)
            {
                foreach (KeyValuePair<string, PNHereNowChannelData> channelData in herenowResult.Channels)
                {
                    Console.WriteLine("---");
                    Console.WriteLine("Channel: " + channelData.Value.ChannelName);
                    Console.WriteLine("Occupancy: " + channelData.Value.Occupancy);

                    if (channelData.Value.Occupants != null)
                    {
                        foreach (var occupant in channelData.Value.Occupants)
                        {
                            Console.WriteLine($"UUID: {occupant.Uuid}");
                            Console.WriteLine($"State: {(occupant.State != null ? pubnub.JsonPluggableLibrary.SerializeToJsonString(occupant.State) : "No state")}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error occurred: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request cannot be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static void HereNowSynchronous()
    {
        // snippet.here_now_synchronous
        pubnub.HereNow()
            // tailor the next two lines to example
            .Channels(new string[] {
                "coolChannel",
                "coolChannel2"
                })
            .IncludeUUIDs(true)
            .Execute(new PNHereNowResultEx(
                (result, status) => {
                    if (status.Error) {
                        // handle error
                        return;
                    }

                    if (result.Channels != null && result.Channels.Count > 0) {
                        foreach (KeyValuePair<string, PNHereNowChannelData> kvp in result.Channels) {
                            PNHereNowChannelData channelData = kvp.Value;
                            Console.WriteLine("---");
                            Console.WriteLine("channel:" + channelData.ChannelName);
                            Console.WriteLine("occupancy:" + channelData.Occupancy);
                            Console.WriteLine("Occupants:");
                            if (channelData.Occupants != null && channelData.Occupants.Count > 0) {
                                for (int index = 0; index < channelData.Occupants.Count; index++) {
                                    PNHereNowOccupantData occupant = channelData.Occupants[index];
                                    Console.WriteLine(string.Format("uuid: {0}", occupant.Uuid));
                                    Console.WriteLine(string.Format("state:{1}", (occupant.State != null) ?
                                    pubnub.JsonPluggableLibrary.SerializeToJsonString(occupant.State) : ""));
                                }
                            }
                        }
                    }
                }
            ));
        // snippet.end
    }

    public static async Task HereNowReturningState()
    {
        // snippet.here_now_returning_state
        PNResult<PNHereNowResult> herenowResponse = await pubnub.HereNow()
            .Channels(new string[] {
                // who is present on those channels?
                "my_channel"
            })
            .IncludeState(true) // include state with request (false by default)
            .IncludeUUIDs(true) // if false, only shows occupancy count
            .ExecuteAsync();

        PNHereNowResult herenowResult = herenowResponse.Result;
        PNStatus status = herenowResponse.Status;
        //handle it
        // snippet.end
    }

    public static async Task HereNowReturnOccupancyOnly()
    {
        // snippet.here_now_return_occupancy_only
        PNResult<PNHereNowResult> herenowResponse = await pubnub.HereNow()
            .Channels(new string[] {
                    // who is present on those channels?
                "my_channel"
            })
            .IncludeState(false) // include state with request (false by default)
            .IncludeUUIDs(false) // if false, only shows occupancy count
            .ExecuteAsync();

        PNHereNowResult herenowResult = herenowResponse.Result;
        PNStatus status = herenowResponse.Status;
        //handle it
        // snippet.end
    }

    public static async Task WhereNowBasicUsage()
    {
        // snippet.where_now_basic_usage
        PNResult<PNWhereNowResult> wherenowResponse = await pubnub.WhereNow()
            .ExecuteAsync();

        PNWhereNowResult wherenowResult = wherenowResponse.Result;
        PNStatus status = wherenowResponse.Status;
        // returns a pojo with channels
        // channel groups which I am part of.
        // snippet.end
    }

    public static void WhereNowSynchronous()
    {
        // snippet.where_now_synchronous
        pubnub.WhereNow()
            .Execute(new PNWhereNowResultExt(
                (result, status) => {
                    // returns a pojo with channels
                    // channel groups which I am part of.
                }
            ));
        // snippet.end
    }

    public static async Task WhereNowOtherUuid()
    {
        // snippet.where_now_other_uuid
        PNResult<PNWhereNowResult> wherenowResponse = await pubnub.WhereNow()
            .Uuid("some-other-uuid") // uuid of the user we want to spy on.
            .ExecuteAsync();

        PNWhereNowResult wherenowResult = wherenowResponse.Result;
        PNStatus status = wherenowResponse.Status;
        // returns a pojo with channels
        // channel groups which "some-other-uuid" part of.ere_now_example_1
        // snippet.end
    }

    public static async Task SetPresenceStateBasicUsage()
    {
        // snippet.set_presence_state_basic_usage
        Dictionary<string, object> myState = new Dictionary<string, object>();
        myState.Add("age", 20);

        PNResult<PNSetStateResult> setstateResponse = await pubnub.SetPresenceState()
            .Channels(new string[] {
                "ch1",
                "ch2",
                "ch3"
            })
            .State(myState)
            .ExecuteAsync();

        PNSetStateResult setstateResult = setstateResponse.Result;
        PNStatus status = setstateResponse.Status;
        // handle set state response
        // snippet.end
    }

    public static async Task GetPresenceStateBasicUsage()
    {
        // snippet.get_presence_state_basic_usage
        PNResult<PNGetStateResult> getstateResponse = await pubnub.GetPresenceState()
            .Channels(new string[] {
                // channels to fetch state for
                "ch1",
                "ch2",
                "ch3"
            })
            .ChannelGroups(new string[] {
                // channel groups to fetch state for
                "cg1",
                "cg2",
                "cg3"
            })
            .Uuid("suchUUID") // uuid of user to fetch, or for own uuid
            .ExecuteAsync();

        PNGetStateResult getstateResult = getstateResponse.Result;
        PNStatus status = getstateResponse.Status;
        // handle response
        // snippet.end
    }

    public static void SetPresenceStateSynchronous()
    {
        // snippet.set_presence_state_synchronous
        Dictionary<string, object> myState = new Dictionary<string, object>();
        myState.Add("age", 20);

        pubnub.SetPresenceState()
            .Channels(new string[] {
                "ch1",
                "ch2",
                "ch3"
            })
            .State(myState)
            .Execute(new PNSetStateResultExt(
                (result, status) => {
                    // handle set state response
                }
            ));
        // snippet.end
    }

    public static void GetPresenceStateSynchronous()
    {
        // snippet.get_presence_state_synchronous
        pubnub.GetPresenceState()
            .Channels(new string[] {
                // channels to fetch state for
                "ch1",
                "ch2",
                "ch3"
            })
            .ChannelGroups(new string[] {
                // channel groups to fetch state for
                "cg1",
                "cg2",
                "cg3"
            })
            .Uuid("suchUUID") // uuid of user to fetch, or for own uuid
            .Execute(new PNGetStateResultExt(
                (result, status) => {
                    // handle response
                }
            ));
        // snippet.end
    }

    public static async Task SetPresenceStateForChannelGroups()
    {
        // snippet.set_presence_state_for_channel_groups
        Dictionary<string, object> myState = new Dictionary<string, object>();
        myState.Add("age", 20);

        PNResult<PNSetStateResult> setstateResponse = await pubnub.SetPresenceState()
            .ChannelGroups(new string[] {
                // apply on those channel groups
                "cg1",
                "cg2",
                "cg3"
            })
            .Channels(new string[] {
                // apply on those channels
                "ch1",
                "ch2",
                "ch3"
            })
            .State(myState) // the new state
            .ExecuteAsync();

        PNSetStateResult setstateResult = setstateResponse.Result;
        PNStatus status = setstateResponse.Status;
        // on new state for those channels
        // snippet.end
    }
} 