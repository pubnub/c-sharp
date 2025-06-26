using System;
using PubnubApi;

namespace PubnubExamples
{
    /// <summary>
    /// Example demonstrating the new PubNub C# SDK with System.Text.Json
    /// for reduced storage footprint and better performance
    /// </summary>
    public class SystemTextJsonExample
    {
        public static void Main()
        {
            // PubNub now uses System.Text.Json by default (lighter, faster, smaller footprint)
            var config = new PNConfiguration(new UserId("myUserId"))
            {
                SubscribeKey = "your-subscribe-key",
                PublishKey = "your-publish-key"
            };

            // Create PubNub instance (now uses System.Text.Json by default)
            var pubnub = new Pubnub(config);

            Console.WriteLine("PubNub instance created successfully!");
            Console.WriteLine($"Using System.Text.Json - Instance: {pubnub.InstanceId}");
            Console.WriteLine("Benefits: Smaller package size, better performance, reduced memory usage");

            // Example usage
            DemonstrateUsage(pubnub, "System.Text.Json");
        }

        private static void DemonstrateUsage(Pubnub pubnub, string jsonLibrary)
        {
            Console.WriteLine($"\n--- Using {jsonLibrary} ---");

            // Subscribe to a channel
            pubnub.Subscribe<string>()
                .Channels(new[] { "demo_channel" })
                .Execute();

            // Publish a message
            pubnub.Publish()
                .Channel("demo_channel")
                .Message("Hello from " + jsonLibrary)
                .Execute(new PNCallback<PNPublishResult>((result, status) =>
                {
                    if (status.Error)
                    {
                        Console.WriteLine($"Publish Error: {status.ErrorData?.Information}");
                    }
                    else
                    {
                        Console.WriteLine($"Message published successfully using {jsonLibrary}");
                    }
                }));

            // Get time
            pubnub.Time().Execute(new PNCallback<PNTimeResult>((result, status) =>
            {
                if (status.Error)
                {
                    Console.WriteLine($"Time Error: {status.ErrorData?.Information}");
                }
                else
                {
                    Console.WriteLine($"Server time ({jsonLibrary}): {result.Timetoken}");
                }
            }));
        }
    }
} 