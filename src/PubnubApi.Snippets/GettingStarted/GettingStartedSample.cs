// snippet.using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PubnubApi;
// snippet.end

namespace PubNubGetStarted
{
    public class Program
    {
        private static Pubnub pubnub;

        
        public static async Task Main(string[] args)
        {   
            // snippet.initialize_pubnub
            // Configure PubNub
            PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId")) 
            {
                SubscribeKey = "demo", // Use your own keys in production
                PublishKey = "demo",   // Use your own keys in production
                Secure = true
            };

            // Initialize PubNub
            pubnub = new Pubnub(pnConfiguration);

            Console.WriteLine("PubNub Initialized!");

            // snippet.end

            // Add listeners
            SetupListeners();

            // Subscribe to channel
            SubscribeToChannel("my_channel");

            // Publish a test message
            var message = new Dictionary<string, object> { { "text", "Hello from C# Console!" } };
            await PublishMessageAsync("my_channel", message);

            // The application will continue to run and receive messages 
            // until the user presses a key to exit
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            // Cleanup before exiting
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            Console.WriteLine("PubNub Destroyed.");
        }

        // snippet.setup_event_listeners
        // Set up listeners for messages, presence, and status events
        static void SetupListeners() 
        {
            var listener = new SubscribeCallbackExt(
                // Handle Message Events
                (pn, messageEvent) =>
                {
                    Console.WriteLine($"Message Received: Channel={messageEvent.Channel}, Message={messageEvent.Message}");

                    var messageData = messageEvent.Message as Dictionary<string, object>;
                    if (messageData != null && messageData.ContainsKey("text"))
                    {
                        Console.WriteLine($"Parsed Text: {messageData["text"]}");
                    }
                },
                // Handle Presence Events (optional)
                (pn, presenceEvent) =>
                {
                    Console.WriteLine($"Presence Event: Channel={presenceEvent.Channel}, Event={presenceEvent.Event}, UUID={presenceEvent.Uuid}");
                },
                // Handle Status Events
                (pn, statusEvent) =>
                {
                    if (statusEvent.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        Console.WriteLine($"Connected to PubNub on channel(s): {string.Join(",", statusEvent.AffectedChannels)}");
                    }
                    else if (statusEvent.Category == PNStatusCategory.PNSubscriptionChangedCategory)
                    {
                        Console.WriteLine($"Subscription changed,Now connected to PubNub on channel(s): {string.Join(",", statusEvent.AffectedChannels)}");
                    }
                    else if (statusEvent.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        Console.WriteLine("Disconnected from PubNub.");
                    }
                    else if (statusEvent.Error)
                    {
                        Console.WriteLine($"PubNub Status Error: {statusEvent.ErrorData.Information}");
                    }
                }
            );
            pubnub.AddListener(listener);
            Console.WriteLine("PubNub Listeners Set Up.");
        }
        // snippet.end

        // snippet.create_subscription
        // Subscribe to a channel to receive messages
        static void SubscribeToChannel(string channelName)
        {
            pubnub.Subscribe<string>() // Use object if message type is unknown/mixed
                .Channels(new string[] { channelName })
                .WithPresence() // Enable presence events 
                .Execute();

            Console.WriteLine($"Subscribed to channel: {channelName}");
        }
        // snippet.end

        // snippet.publish_messages
        // Publish a message to a channel using async/await
        static async Task PublishMessageAsync(string channelName, Dictionary<string, object> message)
        {
            try
            {
                var result = await pubnub.Publish()
                    .Channel(channelName)
                    .Message(message)
                    .ExecuteAsync();

                if (!result.Status.Error)
                {
                    Console.WriteLine($"Message Published! Timetoken: {result.Result.Timetoken}");
                }
                else
                {
                    Console.WriteLine($"Publish Error: {result.Status.ErrorData.Information}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during publish: {ex.Message}");
            }
        }
        // snippet.end
    }

} 