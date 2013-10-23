# PubNub 3.5 Web Data Push Cloud-Hosted API
# C# for .net 4.0/4.5 and 3.5

Open 3.5/PubNub-Messaging/PubNub-Messaging.sln, and the example files Pubnub_Example.cs and PubnubMultiChannel.cs should demonstrate all functionality, asyncronously using delegates.
You can also view and inspect the tests for additional insight. 

NuGet usage and example screencast is also available here: https://vimeo.com/pubnub/videos/search:.net/sort:date/format:detail

## Object Cleanup

For best performance after completion of all intended operations, please call the EndPendingRequests() method
of the Pubnub instance, and assign it to null. This will help ensure speedy resources cleanup when you are done
with the object.

### Important Change in PubNub 3.5 
1. Error Callback method signature changed. PubnubClientError object will be passed to error callback with error details. So all operations/non-operation method signatures,which are using error callback method, were modified.
2. PubNub Access Manager (PAM) to grant/revoke/audit access to channels.

### Note
Currently all the generic methods support string and object types only. Strong types will be supported in future.

### Example Code on presence, subscribe and unsubscribing multiple channels



```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

public class PubnubMultiChannel
    {
        static public Pubnub pubnub;

        static ConcurrentDictionary<string, ManualResetEvent> mrePresenceConnect = new ConcurrentDictionary<string, ManualResetEvent>();
        static ConcurrentDictionary<string, ManualResetEvent> mrePresenceMessage = new ConcurrentDictionary<string, ManualResetEvent>();

        static ConcurrentDictionary<string, ManualResetEvent> mreSubscribeConnect = new ConcurrentDictionary<string, ManualResetEvent>();

        static ConcurrentDictionary<string, ManualResetEvent> mreUnsubscribeDisconnect = new ConcurrentDictionary<string, ManualResetEvent>();

        static ConcurrentDictionary<string, ManualResetEvent> mrePresenceUnsubscribeDisconnect = new ConcurrentDictionary<string, ManualResetEvent>();

        static ConcurrentDictionary<string, bool> presenceChannelConnected = new ConcurrentDictionary<string, bool>();
        static ConcurrentDictionary<string, bool> subscribeChannelConnected = new ConcurrentDictionary<string, bool>();
        static ConcurrentDictionary<string, bool> unsubscribeChannelDisconnected = new ConcurrentDictionary<string, bool>();
        static ConcurrentDictionary<string, bool> presenceUnsubscribeChannelDisconnected = new ConcurrentDictionary<string, bool>();

        static public void Main()
        {
            pubnub = new Pubnub("demo", "demo");
            Console.WriteLine("Welcome to usage example for presence/subscribe/unsubscribe");

            PresenceAndSubscribeUsageExample();

            Console.WriteLine("");
            Console.WriteLine("Terminating Pubnub Instance");
            pubnub.EndPendingRequests();
            pubnub = null;

            Console.WriteLine("");
            Console.WriteLine("Press any key to exit");
            Console.WriteLine("");


            Console.ReadLine();
        }

        static void PresenceAndSubscribeUsageExample()
        {
            Console.WriteLine("Example Start");
            //Sign up for presence with multiple channels with comma delimiter = "firstchannel,secondchannel,thirdchannel"
            string[] channels = {"firstchannel","secondchannel","thirdchannel"}; //Multiple Channels
            //string[] channels = { "singlechannel" }; //Single Channel
            foreach (string channel in channels)
            {
                mrePresenceConnect.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
                mrePresenceMessage.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
                mreSubscribeConnect.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
                mreUnsubscribeDisconnect.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
                mrePresenceUnsubscribeDisconnect.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));

                presenceChannelConnected[channel] = false;
                subscribeChannelConnected[channel] = false;
                unsubscribeChannelDisconnected[channel] = false;
                presenceUnsubscribeChannelDisconnected[channel] = false;
            }

            Console.WriteLine("");
            Console.WriteLine("Signing up presence channels...");
            Console.WriteLine("");
            pubnub.Presence<string>(string.Join(",",channels), PresenceRegularCallback, PresenceConnectCallback, CommonErrorCallback);
            WaitHandle.WaitAll(mrePresenceConnect.Values.ToArray());
            foreach (string channel in channels)
            {
                if (!presenceChannelConnected[channel])
                {
                    Console.WriteLine("** Presence connect missing for channel = {0} **", channel);
                }
            }
            Console.WriteLine("");
            Console.WriteLine("Signing up presence channels...DONE");
            Console.WriteLine("");

            Console.WriteLine("Subscribing channels...");
            Console.WriteLine("");
            pubnub.Subscribe<string>(string.Join(",", channels), SubscribeRegularCallback, SubscribeConnectCallback, CommonErrorCallback);
            WaitHandle.WaitAll(mreSubscribeConnect.Values.ToArray());
            foreach (string channel in channels)
            {
                if (!subscribeChannelConnected[channel])
                {
                    Console.WriteLine("Subscribe connect missing for channel = {0}", channel);
                }
            }
            WaitHandle.WaitAll(mrePresenceMessage.Values.ToArray());
            Console.WriteLine("");
            Console.WriteLine("Subscribing channels...DONE");
            Console.WriteLine("");

            foreach (string channel in channels)
            {
                mrePresenceMessage[channel].Reset();
            }

            Console.WriteLine("Unsubscribing channels...");
            Console.WriteLine("");
            pubnub.Unsubscribe<string>(string.Join(",", channels), UnsubscribeRegularCallback, UnsubscribeConnectCallback, UnsubscribeDisconnectCallback, CommonErrorCallback);
            WaitHandle.WaitAll(mreUnsubscribeDisconnect.Values.ToArray());
            foreach (string channel in channels)
            {
                if (!unsubscribeChannelDisconnected[channel])
                {
                    Console.WriteLine("Unsubscribe disconnect missing for channel = {0}", channel);
                }
            }

            WaitHandle.WaitAll(mrePresenceMessage.Values.ToArray());
            Console.WriteLine("");
            Console.WriteLine("Unsubscribing channels...DONE");
            Console.WriteLine("");
            Console.WriteLine("NOTE: Sometimes when unsubscribed channels, you may notice additional \njoin and leave event for each channel, which is normal");
            Console.WriteLine("");

            Console.WriteLine("Presence Unsubscribe...");
            Console.WriteLine("");
            pubnub.PresenceUnsubscribe<string>(string.Join(",", channels), PresenceUnsubscribeRegularCallback, PresenceUnsubscribeConnectCallback, PresenceUnsubscribeDisconnectCallback, CommonErrorCallback);
            WaitHandle.WaitAll(mrePresenceUnsubscribeDisconnect.Values.ToArray());
            foreach (string channel in channels)
            {
                if (!presenceUnsubscribeChannelDisconnected[channel])
                {
                    Console.WriteLine("Presence Unsubscribe disconnect missing for channel = {0}", channel);
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Presence Unsubscribe...DONE");
            Console.WriteLine("");

            Console.WriteLine("Example End");
        }

        static void PresenceRegularCallback(string result)
        {
            //When any subscriber channels subscribe/unsubscribe, we get "join" or "leave" events in this callback
            Console.WriteLine(result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (serializedMessage.Length > 0)
                {
                    string channel = serializedMessage[2].ToString();
                    if (mrePresenceMessage.ContainsKey(channel))
                    {
                        mrePresenceMessage[channel].Set();
                    }
                }
            }
            
        }

        static void PresenceConnectCallback(string result)
        {
            Console.WriteLine(result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    int statusCode = Int32.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    string channel = (string)deserializedResult[2];
                    if (presenceChannelConnected.ContainsKey(channel))
                    {
                        presenceChannelConnected[channel] = true;
                        mrePresenceConnect[channel].Set();
                    }
                }
            }
        }

        static void CommonErrorCallback(PubnubClientError result)
        {
            if (result != null && result.StatusCode != 0)
            {
                Console.WriteLine(result.Description);
            }
        }

        static void SubscribeRegularCallback(string result)
        {
            Console.WriteLine(result);
        }

        static void SubscribeConnectCallback(string result)
        {
            Console.WriteLine(result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    int statusCode = Int32.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    string channel = (string)deserializedResult[2];
                    if (statusCode == 1 && statusMessage.ToLower() == "connected")
                    {
                        if (subscribeChannelConnected.ContainsKey(channel))
                        {
                            subscribeChannelConnected[channel] = true;
                            mreSubscribeConnect[channel].Set();
                        }
                    }
                }
            }
        }

        static void UnsubscribeRegularCallback(string result)
        {
            Console.WriteLine(result);
        }

        static void UnsubscribeConnectCallback(string result)
        {
            Console.WriteLine(result);
        }

        static void UnsubscribeDisconnectCallback(string result)
        {
            Console.WriteLine(result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[] && deserializedResult.Length > 0)
                {
                    string channel = (string)deserializedResult[2];
                    if (unsubscribeChannelDisconnected.ContainsKey(channel))
                    {
                        unsubscribeChannelDisconnected[channel] = true;
                        mreUnsubscribeDisconnect[channel].Set();
                    }
                }
            }
        }

        static void PresenceUnsubscribeRegularCallback(string result)
        {
            Console.WriteLine(result);
        }

        static void PresenceUnsubscribeConnectCallback(string result)
        {
            Console.WriteLine(result);
        }

        static void PresenceUnsubscribeDisconnectCallback(string result)
        {
            Console.WriteLine(result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[] && deserializedResult.Length > 0)
                {
                    string channel = (string)deserializedResult[2];
                    if (presenceUnsubscribeChannelDisconnected.ContainsKey(channel))
                    {
                        presenceUnsubscribeChannelDisconnected[channel] = true;
                        mrePresenceUnsubscribeDisconnect[channel].Set();
                    }
                }
            }
        }

    
    }

```

## Running the Interactive Demo App

1. Open up the solution file
2. Right click on PubNub-Messaging, and set as Startup Projet
3. CTRL-F5 or F5 to run it

## Running the Tests

1. Install NUnit from http://www.nunit.org/index.php?p=download (2.6.2 is reccomended, but current/later versions should be ok)
2. Restart VS
3. Open up the solution file
4. Right click on PubNub-Messaging.Tests, and set as Startup Projet
5. Save the project
5. CTRL-F5 or F5 to run it

## Switching between .net 3.5 and 4

1. Open up the solution file
2. Right click on PubNub-x (where x is Messaging or Messaging.Tests), and set as Startup Projet
3. Right click on PubNub-x, select properties
4. Click the Application Tab
5. Set Target Framework to 3.5 or 4
6. Save the project
7. CTRL-F5 or F5 to run it

Report an issue, or email us at support if there are any additional questions or comments.
