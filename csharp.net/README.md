# PubNub 3.5 Web Data Push Cloud-Hosted API
# C# for .net 4.0/4.5 and 3.5

Open 3.5/PubNub-Messaging/PubNub-Messaging.sln, and the example files Pubnub_Example.cs and PubnubMultiChannel.cs should demonstrate all functionality, asyncronously using delegates.
You can also view and inspect the tests for additional insight. 

NuGet usage and example screencast is also available here: https://vimeo.com/pubnub/videos/search:.net/sort:date/format:detail

## Object Cleanup

For best performance after completion of all intended operations, please call the EndPendingRequests() method
of the Pubnub instance, and assign it to null. This will help ensure speedy resources cleanup when you are done
with the object.

### Important Changes in PubNub 3.5 
1. The Error Callback method signature has changed. Now, instead of a string, a PubnubClientError object will be passed to a callback of choice with error details (via the error object). To support this, all operation method signatures are modified.
2. PubNub Access Manager (PAM) to grant/revoke/audit access to channels.
3. Fixed bug where sometimes when machine would sleep/resume it would create a bad presence state.

### Generic method type support
Currently all the generic methods support string and object types only. Strong types will be supported in future.

## Sample Code
A completel running demo app which includes the sample code below is available in the PubNub Message project [by way of the 
PubnubExample.cs](3.5/PubNub-Messaging/PubnubExample.cs) file.  [The tests](3.5/PubNub-Messaging.Tests) are also a great reference.

### Instantiate a Pubnub instance

```c#
//Basic usage for subscribe and publish
Pubnub pubnub = new Pubnub(publishKey="demo", subscribeKey="demo");

//optionally, with secret key
Pubnub pubnub = new Pubnub(publishKey="demo", subscribeKey="demo", secretKey);

//optionally, with SSL and cipher key. This would enable encryption/decryption. enableSSL is boolean to toggle HTTP(S).
Pubnub pubnub = new Pubnub(publishKey="demo", subscribeKey="demo", secretKey, cipherKey, enableSSL);

```

### Subscribe to a channel

```c#
pubnub.Subscribe<string>(channel="mychannel", DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
// NOTE: DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage and DisplayErrorMessage are callback methods
```

### Subscribe to presence channel

```c#
pubnub.Presence<string>(channel="mychannel", DisplayPresenceReturnMessage, DisplayPresenceConnectStatusMessage, DisplayErrorMessage);
// NOTE: DisplayPresenceReturnMessage, DisplayPresenceConnectStatusMessage and DisplayErrorMessage are callback methods
```

### Publish a message

```c#
pubnub.Publish<string>(channel="mychannel", publishMsg="My favorite message", DisplayReturnMessage, DisplayErrorMessage);
// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Get history

```c#
// Detailed History for previously published messages. Maximum records returned per request is = 100
pubnub.DetailedHistory<string>(channel="mychannel", recordCountToRetrieve=100, DisplayReturnMessage, DisplayErrorMessage);

// Detailed History from a specific time, ordered old to new messages.
pubnub.DetailedHistory<string>(pubnubChannel, starttime=13557486057035336, DisplayReturnMessage, DisplayErrorMessage, reverse=true);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Who is Here, Now on this channel (HereNow)

```c#
pubnub.HereNow<string>(channel="mychannel", DisplayReturnMessage, DisplayErrorMessage);
// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Unsubscribe from a channel

```c#
pubnub.Unsubscribe<string>(channel="mychannel", DisplayReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage, DisplayErrorMessage);
// NOTE: DisplayReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage and DisplayErrorMessage are callback methods
```

### Unsubscribe from a Presence channel

```c#
pubnub.PresenceUnsubscribe<string>(channel="mychannel", DisplayReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage, DisplayErrorMessage);
// NOTE: DisplayReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage and DisplayErrorMessage are callback methods
```

### PubNub system Time

```c#
pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Filtering / Detecting state from within your error callback

```c#
static void DisplayErrorMessage(PubnubClientError pubnubError)
{
  Console.WriteLine(pubnubError.StatusCode)
  
  //Based on the severity of the error, we can filter out errors for handling or logging.
  switch (pubnubError.Severity)
  {
    case PubnubErrorSeverity.Critical:
      //This type of error needs to be handled.
      break;
    case PubnubErrorSeverity.Warn:
      //This type of error needs to be handled
      break;
    case PubnubErrorSeverity.Info:
      //This type of error can be ignored
      break;
    default:
      break;
  }

  Console.WriteLine(pubnubError.StatusCode); //Unique ID of the error

  Console.WriteLine(pubnubError.Message); //Message received from client or server. From client, it could be from .NET exception.

  if (pubnubError.DetailedDotNetException != null)
  {
    Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
    Console.WriteLine(pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
  }

  Console.WriteLine(pubnubError.MessageSource); // Did this originate from Server or Client-side logic

  if (pubnubError.PubnubWebRequest != null)
  {
    //Captured Web Request details
    Console.WriteLine(pubnubError.PubnubWebRequest.RequestUri.ToString()); 
    Console.WriteLine(pubnubError.PubnubWebRequest.Headers.ToString()); 
  }

  if (pubnubError.PubnubWebResponse != null)
  {
    //Captured Web Response details
    Console.WriteLine(pubnubError.PubnubWebResponse.Headers.ToString());
  }

  Console.WriteLine(pubnubError.Description); // Useful for logging and troubleshooting and support
  Console.WriteLine(pubnubError.Channel); //Channel name(s) at the time of error
  Console.WriteLine(pubnubError.ErrorDateTimeGMT); //GMT time of error

}
```

### Checking the status of a published message via the callback

```c#
private static void DisplayReturnMessage(string publishResult)
{
  if (!string.IsNullOrEmpty(publishResult) && !string.IsNullOrEmpty(publishResult.Trim()))
  {
    object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToObject(publishResult) as object[];
    if (deserializedMessage is object[] && deserializedMessage.Length == 3)
    {
      long statusCode = Int64.Parse(deserializedMessage[0].ToString());
      string statusMessage = (string)deserializedMessage[1];
      string channelName = (string)deserializedMessage[2];

      if (statusCode == 1 && statusMessage.ToLower() == "sent")
      {
        Console.WriteLine("Cool. Messaage Published");
      }
      else
      {
        Console.WriteLine("Oops. Some problem."); 
      }
    }
  }
}
```


### Check the subscribe status via the callback

```
private static void DisplaySubscribeConnectStatusMessage(string result)
{
  if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
  {
    object[] deserializedResult = pubnub.JsonPluggableLibrary.DeserializeToObject(result) as object[];
    if (deserializedResult is object[])
    {
      int statusCode = Int32.Parse(deserializedResult[0].ToString());
      string statusMessage = (string)deserializedResult[1];
      string channel = (string)deserializedResult[2];
      if (statusCode == 1 && statusMessage.ToLower() == "connected")
      {
        Console.WriteLine("Now we are good to receive published messages");
      }
    }
  }
}
```

### PAM: Grant and revoke access for subscribes

```c#
// At the sub-key level (no channel is given)
// Grant
pubnub.GrantAccess<string>("", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);
// Revoke
pubnub.GrantAccess<string>("", read=false, write=false, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

// At the channel level
// Grant
pubnub.GrantAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);
// Revoke
pubnub.GrantAccess<string>(channel="mychannel", read=false, write=false, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);


// At the user level. User is ID'ed via the auth key parameter.
pubnub.AuthenticationKey = authKey;
// Grant
pubnub.GrantAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);
// Revoke
pubnub.GrantAccess<string>(channel="mychannel", read=false, write=false, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```


### PAM: Grant and revoke access for Presence

```c#
// At the sub-key level (no channel is given)
// Grant
pubnub.GrantPresenceAccess<string>("", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);
// Revoke
pubnub.GrantPresenceAccess<string>("", read=false, write=false, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

// At the channel level
// Grant
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);
// Revoke
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=false, write=false, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

// At the user level. User is ID'ed via the auth key parameter.
pubnub.AuthenticationKey = authKey;
// Grant
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);
// Revoke
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Code snippet to audit PAM access for subscribe and presence channels

```
// Audit access at the sub-key level (no channel is given)
pubnub.AuditAccess<string>("",DisplayReturnMessage, DisplayErrorMessage);
pubnub.AuditPresenceAccess<string>("",DisplayReturnMessage, DisplayErrorMessage);

//Audit Access at the channel level
pubnub.AuditAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);
pubnub.AuditPresenceAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);

//Audit Access at the user level. User is ID'ed via the auth key parameter.
pubnub.AuthenticationKey = authKey;
pubnub.AuditAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);
pubnub.AuditPresenceAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);

NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
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
