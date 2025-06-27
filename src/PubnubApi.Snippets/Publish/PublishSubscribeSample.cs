// snippet.using
using PubnubApi;

// snippet.end

public class PublishSubscribeSample
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

    public static async Task PublishBasicUsage()
    {
        // snippet.publish_basic_usage
        try
        {
            // Publishing a message to a channel
            Dictionary<string, float> position = new Dictionary<string, float>
            {
                { "lat", 32F },
                { "lng", 32F }
            };

            Console.WriteLine("before pub: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(position));

            PNResult<PNPublishResult> publishResponse = await pubnub.Publish()
                .Message(position)
                .Channel("my_channel")
                .CustomMessageType("text-message")
                .ExecuteAsync();

            PNPublishResult publishResult = publishResponse.Result;
            PNStatus status = publishResponse.Status;

            if (!status.Error)
            {
                Console.WriteLine("pub timetoken: " + publishResult.Timetoken.ToString());
                Console.WriteLine("pub status code : " + status.StatusCode.ToString());
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

    public static void PublishSynchronous()
    {
        // snippet.publish_synchronous
        //Publishing Dictionary
        Dictionary<string, float> position = new Dictionary<string, float>();
        position.Add("lat", 32F);
        position.Add("lng", 32F);

        Console.WriteLine("before pub: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(position));

        pubnub.Publish()
            .Message(position)
            .Channel("my_channel")
            .CustomMessageType("text-message")
            .Execute(new PNPublishResultExt(
                (result, status) =>
                {
                    Console.WriteLine("pub timetoken: " + result.Timetoken.ToString());
                    Console.WriteLine("pub status code : " + status.StatusCode.ToString());
                }
            ));
        // snippet.end
    }

    public static void PublishWithMetadata()
    {
        // snippet.publish_with_metadata
        string[] arrayMessage = new string[]
        {
            "hello",
            "there"
        };

        pubnub.Publish()
            .Message(arrayMessage.ToList())
            .Channel("suchChannel")
            .ShouldStore(true)
            .Meta(new Dictionary<string, object>() { { "someKey", "someValue" } })
            .UsePOST(true)
            .CustomMessageType("text-message")
            .Execute(new PNPublishResultExt(
                (result, status) =>
                {
                    // handle publish result, status always present, result if successful
                    // status.Error to see if error happened
                }
            ));
        // snippet.end
    }

    public static async Task PublishStoreForHours()
    {
        // snippet.publish_store_for_hours
        var publishResult = await pubnub.Publish()
            .Channel("coolChannel")
            .Message("test")
            .ShouldStore(true)
            .Ttl(10)
            .ExecuteAsync();
        // snippet.end
    }

    // snippet.publish_mobile_payload

    public class MobilePayload
    {
        public Dictionary<string, object> pn_apns;
        public Dictionary<string, object> pn_gcm;
        public Dictionary<string, object> full_game;
    }

    public static void PublishMobilePayload()
    {
        Dictionary<string, object> apnsData = new Dictionary<string, object>();
        apnsData.Add("aps", new Dictionary<string, object>()
        {
            { "alert", "Game update 49ers touchdown" },
            { "badge", 2 }
        });
        apnsData.Add("teams", new string[] { "49ers", "raiders" });
        apnsData.Add("score", new int[] { 7, 0 });

        Dictionary<string, object> gcmData = new Dictionary<string, object>();
        gcmData.Add("data", new Dictionary<string, object>()
        {
            {
                "summary", "Game update 49ers touchdown"
            },
            {
                "teams", new string[] { "49ers", "raiders" }
            },
            {
                "score", new int[] { 7, 0 }
            },
            {
                "lastplay", "5yd run up the middle"
            },
        });

        MobilePayload mobilePayload = new MobilePayload();
        mobilePayload.pn_apns = apnsData;
        mobilePayload.pn_gcm = gcmData;
        mobilePayload.full_game = new Dictionary<string, object>()
        {
            { "date", "2014.05.20" },
            { "foobar", "Data that is not pertinent to devices" }
        };

        pubnub.Publish()
            .Message(mobilePayload)
            .Channel("my_channel")
            .ShouldStore(true)
            .CustomMessageType("text-message")
            .Execute(new PNPublishResultExt(
                (result, status) =>
                {
                    // Check whether request successfully completed or not.
                    if (status.Error)
                    {
                        // something bad happened.
                        Console.WriteLine(
                            $"error while publishing: {pubnub.JsonPluggableLibrary.SerializeToJsonString(status)}");
                    }
                    else
                    {
                        Console.WriteLine($"published with timetoken: {result.Timetoken}");
                    }
                }
            ));
    }
    // snippet.end

    public static void FireBasicUsage()
    {
        // snippet.fire_basic_usage
        string[] arrMessage = new string[]
        {
            "hello",
            "there"
        };

        pubnub.Fire()
            .Message(arrMessage.ToList())
            .Channel("my-channel")
            .UsePOST(true)
            .Execute(new PNPublishResultExt(
                (result, status) =>
                {
                    if (status.Error)
                    {
                        Console.WriteLine(
                            $"error while publishing: {pubnub.JsonPluggableLibrary.SerializeToJsonString(status)}");
                    }
                    else
                    {
                        Console.WriteLine($"published with timetoken: {result.Timetoken}");
                    }
                }
            ));
        // snippet.end
    }

    public static void SignalBasicUsage()
    {
        // snippet.signal_basic_usage
        Dictionary<string, string> myMessage = new Dictionary<string, string>();
        myMessage.Add("msg", "Hello Signals");

        pubnub.Signal()
            .Message(myMessage)
            .Channel("foo")
            .CustomMessageType("text-message")
            .Execute(new PNPublishResultExt((result, status) =>
            {
                if (status.Error)
                {
                    Console.WriteLine(status.ErrorData.Information);
                }
                else
                {
                    Console.WriteLine(result.Timetoken);
                }
            }));
        // snippet.end
    }

    public static void SubscribeBasicUsage()
    {
        // snippet.subscribe_basic_usage
        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                // subscribe to channels
                "my_channel"
            })
            .Execute();
        // snippet.end
    }

    public static void SubscribeNewBasicUsage()
    {
        // snippet.subscribe_basic_usage_new
        Subscription subscription1 = pubnub.Channel("channelName").Subscription();
        subscription1.Subscribe<object>();

        SubscriptionSet subscriptionSet = pubnub.SubscriptionSet(
            new string[] { "channel1", "channel2" },
            new string[] { "channel_group_1", "channel_group_2" },
            SubscriptionOptions.ReceivePresenceEvents
        );

        subscriptionSet.Subscribe<object>();
        // snippet.end
    }

    public static void SubscribeWithLogging()
    {
        // snippet.subscribe_with_logging
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            // subscribeKey from admin panel
            SubscribeKey = "my_subkey", // required
            // publishKey from admin panel (only required if publishing)
            PublishKey = "my_pubkey",
            // logging level declaration
            LogLevel = PubnubLogLevel.Debug
        };

        Pubnub pubnub = new Pubnub(pnConfiguration);

        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                // subscribe to channels
                "my_channel"
            })
            .Execute();
        // snippet.end
    }

    public static void SubscribeMultipleChannels()
    {
        // snippet.subscribe_multiple_channels
        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                // subscribe to channels information
                "my_channel1",
                "my_channel2"
            })
            .Execute();
        // snippet.end
    }

    public static void SubscribeWithPresence()
    {
        // snippet.subscribe_with_presence
        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                // subscribe to channels
                "my_channel"
            })
            .WithPresence() // also subscribe to related presence information
            .Execute();
        // snippet.end
    }

    public static void SubscribeWithState()
    {
        // snippet.subscribe_with_state
        pubnub.AddListener(new SubscribeCallbackExt(
            (pubnubObj, message) => { },
            (pubnubObj, presence) => { },
            (pubnubObj, status) =>
            {
                if (status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("FieldA", "Awesome");
                    data.Add("FieldB", 10);

                    pubnub.SetPresenceState()
                        .Channels(new string[] { "awesomeChannel" })
                        .ChannelGroups(new string[] { "awesomeChannelGroup" })
                        .State(data)
                        .Execute(new PNSetStateResultExt(
                            (r, s) =>
                            {
                                // handle set state response
                            }
                        ));
                }
            }
        ));

        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                "awesomeChannel"
            })
            .Execute();
        // snippet.end
    }

    public static void SubscribeChannelGroup()
    {
        // snippet.subscribe_channel_group
        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                // subscribe to channels
                "ch1",
                "ch2"
            })
            .ChannelGroups(new string[]
            {
                // subscribe to channel groups
                "cg1",
                "cg2"
            })
            .WithTimetoken(1337L) // optional, pass a timetoken
            .WithPresence() // also subscribe to related presence information
            .Execute();
        // snippet.end
    }

    public static void SubscribeChannelGroupPresence()
    {
        // snippet.subscribe_channel_group_presence
        pubnub.Subscribe<string>()
            .ChannelGroups(new string[]
            {
                // subscribe to channel groups
                "cg1",
                "cg2"
            })
            .WithTimetoken(1337L) // optional, pass a timetoken
            .WithPresence() // also subscribe to related presence information
            .Execute();
        // snippet.end
    }

    // snippet.subscribe_custom_type
    public class Phone
    {
        public string Number { get; set; }
        public string Extenion { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PhoneType PhoneType { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Mobile,
        Work
    }

    public static void SubscribeWithCustomType()
    {
        Phone myPhone = new Phone()
        {
            Number = "111-222-2222",
            PhoneType = PhoneType.Mobile,
            Extenion = "11"
        };

        pubnub.Publish()
            .Message(myPhone)
            .Channel("my_channel")
            .ShouldStore(true)
            .Execute(new PNPublishResultExt(
                (result, status) =>
                {
                    // Check whether request successfully completed or not.
                    if (status.Error)
                    {
                        // something bad happened.
                        Console.WriteLine("error happened while publishing: " +
                                          pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                    else
                    {
                        Console.WriteLine("publish worked! timetoken: " + result.Timetoken.ToString());
                    }
                }
            ));

        SubscribeCallbackExt objectListenerSubscribeCallack = new SubscribeCallbackExt(
            (pubnubObj, message) =>
            {
                //message.Message gives the Phone object because you subscribed to type Phone during subscribe.
            },
            (pubnubObj, presence) => { },
            (pubnubObj, status) => { });

        pubnub.AddListener(objectListenerSubscribeCallack);
        pubnub.Subscribe<Phone>()
            .Channels(new string[]
            {
                "my_channel" // subscribe to channels
            })
            .Execute();

        //If you are subscribing to multiple message types, then
        SubscribeCallbackExt stringListenerSubscribeCallack = new SubscribeCallbackExt(
            (pubnubObj, message) =>
            {
                //message.Message gives the string object because you subscribed to type "string" during subscribe.
                string phoneStringMessage = message.Message.ToString(); //this is your string message
                //using pluggable JSON library from the Pubnub instance, but you can use any form of JSON deserialization you wish
                var deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToObject<Phone>(phoneStringMessage);
            },
            (pubnubObj, presence) => { },
            (pubnubObj, status) => { });

        pubnub.AddListener(stringListenerSubscribeCallack);
        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                "my_channel" // subscribe to channels
            })
            .Execute();
    }
    // snippet.end

    public static void CreateSubscriptionSetFromIndividual()
    {
        // snippet.create_subscription_set_from_individual
        // Create a subscription from a channel entity
        Subscription subscription1 = pubnub.Channel("channelName").Subscription();

        // Create a subscription from a channel group entity
        Subscription subscription2 = pubnub.ChannelGroup("channelGroupName").Subscription();

        // create a subscription set from individual entities
        SubscriptionSet subscriptionSet = subscription1.Add(subscription2);

        subscriptionSet.Subscribe<object>();
        // snippet.end
    }

    public static void WildcardSubscribe()
    {
        // snippet.wildcard_subscribe
        pubnub.Subscribe<string>()
            .Channels(new string[]
            {
                // subscribe to channels information
                "foo.*"
            })
            .Execute();
        // snippet.end
    }

    public static void UnsubscribeBasicUsage()
    {
        // snippet.unsubscribe_basic_usage
        pubnub.Unsubscribe<string>()
            .Channels(new string[]
            {
                "my_channel"
            })
            .Execute();
        // snippet.end
    }

    public static void UnsubscribeMultipleChannels()
    {
        // snippet.unsubscribe_multiple_channels
        pubnub.Unsubscribe<string>()
            .Channels(new string[]
            {
                "ch1",
                "ch2",
                "ch3"
            })
            .ChannelGroups(new string[]
            {
                "cg1",
                "cg2",
                "cg3"
            })
            .Execute();
        // snippet.end
    }

    public static void UnsubscribeChannelGroup()
    {
        // snippet.unsubscribe_channel_group
        pubnub.Unsubscribe<string>()
            .ChannelGroups(new string[]
            {
                "cg1",
                "cg2",
                "cg3"
            })
            .Execute();
        // snippet.end
    }

    public static void UnsubscribeAllBasicUsage()
    {
        // snippet.unsubscribe_all_basic_usage
        pubnub.UnsubscribeAll<string>();
        // snippet.end
    }

    public static void AddListenerBasicUsage()
    {
        // snippet.add_listener_basic_usage
        // Add event-specific listeners
        // Add a listener to receive Message changes
        Subscription subscription1 = pubnub.Channel("channelName").Subscription();

        subscription1.onMessage += (Pubnub pn, PNMessageResult<object> messageEvent) =>
        {
            Console.WriteLine($"Message received {messageEvent.Message}");
        };

        subscription1.Subscribe<object>();


        // Add multiple listeners
        SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
            delegate(Pubnub pn, PNMessageResult<object> messageEvent)
            {
                Console.WriteLine($"received message {messageEvent.Message}");
            },
            delegate(Pubnub pn, PNPresenceEventResult e) { Console.WriteLine("Presence event"); },
            delegate(Pubnub pn, PNSignalResult<object> e) { Console.WriteLine("Signal event"); },
            delegate(Pubnub pn, PNObjectEventResult e) { Console.WriteLine("Object event"); },
            delegate(Pubnub pn, PNMessageActionEventResult e) { Console.WriteLine("Message Action event"); },
            delegate(Pubnub pn, PNFileEventResult e) { Console.WriteLine("File event"); }
        );

        Channel firstChannel = pubnub.Channel("first");
        var subscription = firstChannel.Subscription(SubscriptionOptions.ReceivePresenceEvents);
        subscription.AddListener(eventListener);
        subscription.Subscribe<object>();
        // snippet.end
    }

    public static void AddListenersBasicUsage()
    {
        // snippet.add_listeners_basic_usage
        Subscription subscription1 = pubnub.Channel("channelName").Subscription();

        SubscriptionSet subscriptionSet = pubnub.SubscriptionSet(
            new string[] { "channel1", "channel2" },
            new string[] { "channel_group_1", "channel_group_2" },
            SubscriptionOptions.ReceivePresenceEvents
        );

        SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
            delegate(Pubnub pn, PNMessageResult<object> messageEvent)
            {
                Console.WriteLine($"received message {messageEvent.Message}");
            },
            delegate(Pubnub pn, PNPresenceEventResult e) { Console.WriteLine("Presence event"); },
            delegate(Pubnub pn, PNStatus s) { Console.WriteLine("Status event"); }
        );

        subscription1.AddListener(eventListener);
        subscriptionSet.onSignal += (Pubnub pn, PNSignalResult<object> signalEvent) =>
        {
            Console.WriteLine($"Message received {signalEvent.Message}");
        };

        subscription1.Subscribe<object>();
        subscriptionSet.Subscribe<object>();
        // snippet.end
    }

    public static void AddListenerMethod1()
    {
        // snippet.add_listener_method1
        // Adding listener.
        pubnub.AddListener(new SubscribeCallbackExt(
            delegate(Pubnub pnObj, PNMessageResult<object> pubMsg)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg));
                var channelName = pubMsg.Channel;
                var channelGroupName = pubMsg.Subscription;
                var pubTT = pubMsg.Timetoken;
                var msg = pubMsg.Message;
                var publisher = pubMsg.Publisher;
            },
            delegate(Pubnub pnObj, PNPresenceEventResult presenceEvnt)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt));
                var action = presenceEvnt.Event; // Can be join, leave, state-change or timeout
                var channelName = presenceEvnt.Channel; // The channel for which the message belongs
                var occupancy = presenceEvnt.Occupancy; // No. of users connected with the channel
                var state = presenceEvnt.State; // User State
                var channelGroupName =
                    presenceEvnt.Subscription; //  The channel group or wildcard subscription match (if exists)
                var publishTime = presenceEvnt.Timestamp; // Publish timetoken
                var timetoken = presenceEvnt.Timetoken; // Current timetoken
                var uuid = presenceEvnt.Uuid; // UUIDs of users who are connected with the channel
            },
            delegate(Pubnub pnObj, PNSignalResult<object> signalMsg)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(signalMsg));
                var channelName = signalMsg.Channel; // The channel for which the signal belongs
                var channelGroupName =
                    signalMsg.Subscription; // The channel group or wildcard subscription match (if exists)
                var pubTT = signalMsg.Timetoken; // Publish timetoken
                var msg = signalMsg.Message; // The Payload
                var publisher = signalMsg.Publisher; //The Publisher
            },
            delegate(Pubnub pnObj, PNObjectEventResult objectEventObj)
            {
                var channelName = objectEventObj.Channel; // Channel
                var channelMetadata = objectEventObj.ChannelMetadata; //Channel Metadata
                var uidMetadata = objectEventObj.UuidMetadata; // UUID metadata
                var evnt = objectEventObj.Event; // Event
                var type = objectEventObj.Type; // Event type
                if (objectEventObj.Type == "uuid")
                {
                    /* got uuid metadata related event. */
                }
                else if (objectEventObj.Type == "channel")
                {
                    /* got channel metadata related event. */
                }
                else if (objectEventObj.Type == "membership")
                {
                    /* got membership related event. */
                }

                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(objectEventObj));
            },
            delegate(Pubnub pnObj, PNMessageActionEventResult msgActionEvent)
            {
                //handle message action
                var channelName = msgActionEvent.Channel; // The channel for which the message belongs
                var msgEvent = msgActionEvent.Action; // message action added or removed
                var msgActionType = msgActionEvent.Event; // message action type
                var messageTimetoken = msgActionEvent.MessageTimetoken; // The timetoken of the original message
                var actionTimetoken = msgActionEvent.ActionTimetoken; //The timetoken of the message action
            },
            delegate(Pubnub pnObj, PNFileEventResult fileEvent)
            {
                //handle file message event
                var channelName = fileEvent.Channel;
                var chanelGroupName = fileEvent.Subscription;
                var fieldId = (fileEvent.File != null) ? fileEvent.File.Id : null;
                var fileName = (fileEvent.File != null) ? fileEvent.File.Name : null;
                var fileUrl = (fileEvent.File != null) ? fileEvent.File.Url : null;
                var fileMessage = fileEvent.Message;
                var filePublisher = fileEvent.Publisher;
                var filePubTT = fileEvent.Timetoken;
            },
            delegate(Pubnub pnObj, PNStatus pnStatus)
            {
                Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode);
                var affectedChannelGroups =
                    pnStatus.AffectedChannelGroups; // The channel groups affected in the operation, of type array.
                var affectedChannels =
                    pnStatus.AffectedChannels; // The channels affected in the operation, of type array.
                var category = pnStatus.Category; //Returns PNConnectedCategory
                var operation = pnStatus.Operation; //Returns PNSubscribeOperation
            }
        ));
        
        //Add listener to receive Signal messages
        SubscribeCallbackExt signalSubscribeCallback = new SubscribeCallbackExt(
            delegate (Pubnub pubnubObj, PNSignalResult<object> message) {
                // Handle new signal message stored in message.Message
            },
            delegate (Pubnub pubnubObj, PNStatus status)
            {
                // the status object returned is always related to subscribe but could contain
                // information about subscribe, heartbeat, or errors
            }
        );
        pubnub.AddListener(signalSubscribeCallback);
        
        //Add listener to receive Events
        SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
            delegate (Pubnub pnObj, PNObjectEventResult objectEvent)
            {
                string channelMetadataId = objectEvent.Channel; // The channel
                string uuidMetadataId = objectEvent.UuidMetadata.Uuid; // The UUID
                string objEvent = objectEvent.Event; // The event name that occurred
                string eventType = objectEvent.Type; // The event type that occurred
                PNUuidMetadataResult uuidMetadata = objectEvent.UuidMetadata; // UuidMetadata
                PNChannelMetadataResult channelMetadata = objectEvent.ChannelMetadata; // ChannelMetadata
            },
            delegate (Pubnub pnObj, PNStatus status)
            {

            }
        );
        pubnub.AddListener(eventListener);
        // snippet.end
    }

    // snippet.add_listener_method2
    public class DevSubscribeCallback : SubscribeCallback
    {
        public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        {
            // Handle new message stored in message.Message
        }

        public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
        {
            // handle incoming presence data
        }

        public override void Signal<T>(Pubnub pubnub, PNSignalResult<T> signal)
        {
            // Handle new signal message stored in signal.Message
        }

        public override void Status(Pubnub pubnub, PNStatus status)
        {
            // the status object returned is always related to subscribe but could contain
            // information about subscribe, heartbeat, or errors
            // use the PNOperationType to switch on different options
            switch (status.Operation)
            {
                // let's combine unsubscribe and subscribe handling for ease of use
                case PNOperationType.PNSubscribeOperation:
                case PNOperationType.PNUnsubscribeOperation:
                    // note: subscribe statuses never have traditional
                    // errors, they just have categories to represent the
                    // different issues or successes that occur as part of subscribe
                    switch (status.Category)
                    {
                        case PNStatusCategory.PNConnectedCategory:
                            // this is expected for a subscribe, this means there is no error or issue whatsoever
                            break;
                        case PNStatusCategory.PNReconnectedCategory:
                            // this usually occurs if subscribe temporarily fails but reconnects. This means
                            // there was an error but there is no longer any issue
                            break;
                        case PNStatusCategory.PNDisconnectedCategory:
                            // this is the expected category for an unsubscribe. This means there
                            // was no error in unsubscribing from everything
                            break;
                        case PNStatusCategory.PNUnexpectedDisconnectCategory:
                            // this is usually an issue with the internet connection, this is an error, handle appropriately
                            break;
                        case PNStatusCategory.PNAccessDeniedCategory:
                            // this means that Access Manager does allow this client to subscribe to this
                            // channel and channel group configuration. This is another explicit error
                            break;
                        default:
                            // More errors can be directly specified by creating explicit cases for other
                            // error categories of `PNStatusCategory` such as `PNTimeoutCategory` or `PNMalformedFilterExpressionCategory` or `PNDecryptionErrorCategory`
                            break;
                    }

                    break;
                case PNOperationType.PNHeartbeatOperation:
                    // heartbeat operations can in fact have errors, so it is important to check first for an error.
                    if (status.Error)
                    {
                        // There was an error with the heartbeat operation, handle here
                    }
                    else
                    {
                        // heartbeat operation was successful
                    }

                    break;
                default:
                    // Encountered unknown status type
                    break;
            }
        }

        public override void ObjectEvent(Pubnub pubnub, PNObjectEventResult objectEvent)
        {
            // handle incoming user, space and membership event data
        }

        public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageAction)
        {
            // handle incoming message action events
        }

        public override void File(Pubnub pubnub, PNFileEventResult fileEvent)
        {
            // handle incoming file messages
        }
    }

    public static void AddListenerMethod2()
    {
        // Usage of the above listener
        DevSubscribeCallback regularListener = new DevSubscribeCallback();
        pubnub.AddListener(regularListener);
    }
    // snippet.end

    public static void RemoveListener()
    {
        // snippet.remove_listener
        SubscribeCallbackExt listenerSubscribeCallback = new SubscribeCallbackExt(
            (pubnubObj, message) => { },
            (pubnubObj, presence) => { },
            (pubnubObj, status) => { });

        pubnub.AddListener(listenerSubscribeCallback);

        // some time later
        pubnub.RemoveListener(listenerSubscribeCallback);
        // snippet.end
    }

    public static void AddConnectionStatusListener()
    {
        // snippet.add_connection_status_listener
        SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
            delegate(Pubnub pn, PNStatus e) { Console.WriteLine("Status event"); }
        );

        pubnub.AddListener(eventListener);
        // snippet.end
    }

    public static void UnsubscribeNewBasicUsage()
    {
        // snippet.unsubscribe_new_basic_usage
        Subscription subscription1 = pubnub.Channel("channelName").Subscription();

        SubscriptionSet subscriptionSet = pubnub.SubscriptionSet(
            new string[] { "channel1", "channel2" },
            new string[] { "channel_group_1", "channel_group_2" },
            SubscriptionOptions.ReceivePresenceEvents
        );

        subscription1.Subscribe<object>();
        subscriptionSet.Subscribe<object>();

        subscription1.Unsubscribe<object>();
        subscriptionSet.Unsubscribe<object>();
        // snippet.end
    }

    public static void UnsubscribeAllNewBasicUsage()
    {
        // snippet.unsubscribe_all_new_basic_usage
        Subscription subscription1 = pubnub.Channel("channelName").Subscription();

        SubscriptionSet subscriptionSet = pubnub.SubscriptionSet(
            new string[] { "channel1", "channel2" },
            new string[] { "channel_group_1", "channel_group_2" },
            SubscriptionOptions.ReceivePresenceEvents
        );

        subscription1.Subscribe<object>();
        subscriptionSet.Subscribe<object>();

        pubnub.UnsubscribeAll<object>();
        // snippet.end
    }
}