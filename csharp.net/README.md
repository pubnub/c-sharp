# PubNub 3.5 Web Data Push Cloud-Hosted API
# C# for .net 4.0/4.5 and 3.5

Open 3.5/PubNub-Messaging/PubNub-Messaging.sln, and the example Pubnub_Example.cs should demonstrate all functionality, asyncronously using delegates.
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

### Sample Example Code for multiple channels


## Presence

        static ConcurrentDictionary<string, ManualResetEvent> mrePresenceConnect = new ConcurrentDictionary<string, ManualResetEvent>();
        static ConcurrentDictionary<string, ManualResetEvent> mrePresenceMessage = new ConcurrentDictionary<string, ManualResetEvent>();
        static ConcurrentDictionary<string, bool> presenceChannelConnected = new ConcurrentDictionary<string, bool>();
	
	static void PresenceExample()
	{
          //Sign up for presence or subscribe with multiple channels with comma delimiter = "firstchannel,secondchannel,thirdchannel"
  	  string[] channels = {"firstchannel","secondchannel","thirdchannel"}; //Multiple Channels

	  foreach (string channel in channels)
	  {
              mrePresenceConnect.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
              mrePresenceMessage.AddOrUpdate(channel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));

              presenceChannelConnected[channel] = false;
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

pubnub.Subscribe<string>(channel, DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);

Example:
```
Previous => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage);

Migration => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);

NOTE: The callback methods DisplayReturnMessage, DisplayConnectStatusMessage and DisplayErrorMessage(new callback parameter) are used in example code for your review.

```
## Running the Demo App

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
