# PubNub C# SDK

This is the official PubNub C# SDK repository.

PubNub takes care of the infrastructure and APIs needed for the realtime communication layer of your application. Work on your app's logic and let PubNub handle sending and receiving data across the world in less than 100ms.

## Get keys

You will need the publish and subscribe keys to authenticate your app. Get your keys from the [Admin Portal](https://dashboard.pubnub.com/).

## Configure PubNub

1. Integrate PubNub into your project using one of the following NuGet packages:

    * For .Net 3.5/4.0/4.5/4.61: [Pubnub package](https://www.nuget.org/packages/Pubnub/)
    * For Xamarin.Android, Xamarin.iOS and .NetStandard/.NetCore: [PubnubPCL package](https://www.nuget.org/packages/PubnubPCL/)
    * For Universal Windows: [PubnubUWP package](https://www.nuget.org/packages/PubnubUWP/)

1. Create a new class and add the following code:

    ```csharp
    using PubnubApi;

   
   PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"));
   pnConfiguration.SubscribeKey = "mySubscribeKey";
   pnConfiguration.PublishKey = "myPublishKey";
   Pubnub pubnub = new Pubnub(pnConfiguration);
    ```

    This is the minimum configuration you need to send and receive messages with PubNub.

## Add event listeners

```csharp
pubnub.AddListener(new SubscribeCallbackExt(
    delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
    {
        if (pubMsg != null) {
            Debug.WriteLine("In Example, SubscribeCallback received PNMessageResult");
            Debug.WriteLine("In Example, SubscribeCallback messsage channel = " + pubMsg.Channel);
            Debug.WriteLine("In Example, SubscribeCallback messsage channelGroup = " + pubMsg.Subscription);
            Debug.WriteLine("In Example, SubscribeCallback messsage publishTimetoken = " + pubMsg.Timetoken);
            Debug.WriteLine("In Example, SubscribeCallback messsage publisher = " + pubMsg.Publisher);
            string jsonString = pubMsg.Message.ToString();
            Dictionary<string, string> msg = pubnub.JsonPluggableLibrary.DeserializeToObject<Dictionary<string,string>>(jsonString);
            Debug.WriteLine("msg: " + msg["msg"]);
        }
    },
    delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt)
    {
        if (presenceEvnt != null) {
            Debug.WriteLine("In Example, SubscribeCallback received PNPresenceEventResult");
            Debug.WriteLine(presenceEvnt.Channel + " " + presenceEvnt.Occupancy + " " + presenceEvnt.Event);
        }
    }
)
```

## Publish and subscribe

In this code, publishing a message is triggered when the subscribe call is finished successfully. The `Publish()` method uses the `msg` variable that is used in the following code.

```csharp
Dictionary<string, string> message = new Dictionary<string, string>();
    message.Add("msg", "Hello world!");

pubnub.Subscribe<string>()
        .Channels(new string[]{
            "my_channel"
        }).Execute();

PNResult<PNPublishResult> publishResponse = await pubnub.Publish()
                                            .Message(message)
                                            .Channel("my_channel")
                                            .ExecuteAsync();
PNPublishResult publishResult = publishResponse.Result;
PNStatus status = publishResponse.Status;
Console.WriteLine("pub timetoken: " + publishResult.Timetoken.ToString());
Console.WriteLine("pub status code : " + status.StatusCode.ToString());
```

## Documentation

[API reference for C#](https://www.pubnub.com/docs/c-sharp-dot-net-c-sharp/pubnub-c-sharp-sdk)

## Support

If you **need help** or have a **general question**, contact <support@pubnub.com>.
