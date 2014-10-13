# Please direct all Support Questions and Concerns to Support@PubNub.com

## PubNub 3.6 for C Sharp (C#)
### Supporting .net 3.5, 4.0, and 4.5

Open PubNub-Messaging/PubNub-Messaging.csproj, and the example file PubnubExample.cs should demonstrate all functionality, asyncronously using delegates.
You can also view and inspect the tests for additional insight. 

NuGet usage and example screencast is also available here: https://vimeo.com/pubnub/videos/search:.net/sort:date/format:detail

## Object Cleanup

For best performance after completion of all intended operations, please call the EndPendingRequests() method
of the Pubnub instance, and assign it to null. This will help ensure speedy resources cleanup when you are done
with the object.

### Important Changes in PubNub 3.6 
1. Renamed property name HeartbeatInterval to LocalClientHeartbeatInterval.
2. New properties PresenceHeartbeat, PresenceHeartbeatInterval were added.
3. Additional optional parameters showUUIDList, includeUserState for HereNow method.
4. New methods GlobalHereNow, WhereNow, SetUserState, GetUserState, SetLocalUserState, GetLocalUserState and ChangeUUID
5. UserState data in the response format of presence events.


### Important Changes in PubNub 3.5 
1. The Error Callback method signature has changed. Now, instead of a string, a PubnubClientError object will be passed to a callback of choice with error details (via the error object). To support this, all operation method signatures are modified.
2. PubNub Access Manager (PAM) to grant/revoke/audit access to channels.
3. Fixed bug where sometimes when machine would sleep/resume it would create a bad presence state.

### Generic method type support
Currently all the generic methods support string and object types only. Strong types will be supported in future.

## Sample Code
A completel running demo app which includes the sample code below is available in the PubNub Message project [by way of the 
PubnubExample.cs](PubNub-Messaging/PubnubExample.cs) file.  [The tests](/PubNub-Messaging.Tests) are also a great reference.

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

pubnub.HereNow<string>(channel="mychannel", showUUID=true, includeUserState=true, DisplayReturnMessage, DisplayErrorMessage);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Set the state of the user on this channel (SetUserState)

```c#
pubnub.SetUserState<string>(channel="mychannel", jsonUserState="{mychannel:{"key1":"value1"}}", DisplayReturnMessage, DisplayErrorMessage);

pubnub.SetUserState<string>(channel="mychannel", uuid="myuuid", jsonUserState='{mychannel:{"key1":"value1"}}', DisplayReturnMessage, DisplayErrorMessage);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Get the state of the user on this channel (SetUserState)

```c#
pubnub.GetUserState<string>(channel="mychannel", DisplayReturnMessage, DisplayErrorMessage);

pubnub.GetUserState<string>(channel="mychannel", uuid="myAlternateUUID", DisplayReturnMessage, DisplayErrorMessage);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Current channels for the given subscriber (WhereNow)

```c#
pubnub.WhereNow<string>(whereNowUuid="myuuid", DisplayReturnMessage, DisplayErrorMessage);

// NOTE: DisplayReturnMessage and DisplayErrorMessage are callback methods
```

### Current subscriber list for subkey (GlobalHereNow)

```c#
pubnub.GlobalHereNow<string>(showUUID=true, includeUserState=true,DisplayReturnMessage, DisplayErrorMessage);

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

### Logging

To capture the error/info messages from ErrorCallback, set config file appSettings key PubnubMessaging.PubnubErrorFilterLevel = "3" (Info). Other available values are 2 (Warning) and 1 (Critical). If config file support is not available for your client, refer errorLevel variable from Variables Reference section.
For troubleshooting purpose, low level logging option is also available. For this set config file appSettings key PubnubMessaging.LogLevel = "3" (Verbose). Other available values are 0 (Off), 1 (Error), 2 (Info) and 4 (Warning). If config file support is not available for your client, refer pubnubLogLevel variable from Variables Reference section.



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


## Variables Reference

```c#
overrideTcpKeepAlive = false 
```

This variable default value is set to false to consider "request.ServicePoint.SetTcpKeepAlive()" method in the code. For mono framework 2.10.9 stable release, SetTcpKeepAlive() is not supported. To support Mono, set the value of "overrideTcpKeepAlive" to true
 
```c# 
_pubnubWebRequestCallbackIntervalInSeconds = 310 
```

This variable sets the time limit in seconds for the web request to run. Applies to subscribe and presence web requests. In the example, we terminate HTTP requests after 310 seconds of not hearing back from the server.

```c#
_pubnubOperationTimeoutIntervalInSeconds = 15
```

This variable sets the time limit in seconds for the web request to run. Applies to regular operation requests like time, publish, "here now" and detailed history. In the example, we terminate HTTP requests after 15 seconds of not hearing back from the server.

```c#
_pubnubNetworkTcpCheckIntervalInSeconds = 15 
```

This variable sets the time interval(heart-beat) to check internet/network/tcp connection for HTTP requests when an active request is initiated. In the example, we check network/tcp connection every 15 seconds. It is also used for re-connect interval when "overrideTcpKeepAlive" = true (for Mono framework 2.10.9). Re-connect applies only for subscribe and presence.

```c#
_pubnubNetworkCheckRetries = 50 
```

This variable is to set the maximum number of re-tries for re-connect to check internet/network connection for subscribe and presence. In the example, we attempt 50 times to check connection.

```c# 
_pubnubWebRequestRetryIntervalInSeconds = 10 
```

This variable is to set the wait time for re-subscribe and re-presence, if the previous subscribe or presence fail. This variable is valid only when "overrideTcpKeepAlive" = false
 
If there is no internet/network connection after "pubnubNetworkCheckRetries" attempts for subscribe, "Unsubscribed after 50 failed retries" message will be sent and unsubscribe occurs automatically. Similary for presence, "Presence-unsubscribed after 50 failed retries"
 
For publish, here_now, detailed history and time, there is no attempt to re-connect. If the request fails due to http web request timeout, "Operation timeout" error be sent. If there is network/internet disconnect, error message "Network connect error" will be sent. 

```c#
_pubnubPresenceHeartbeatInSeconds = 63
```

This variable is to set the heartbeat for the subscribed channel for presence before expiry. In the example, we indicate that subsciber can expire after 63 seconds if no heartbeat request is received by server.

```c#
_presenceHeartbeatIntervalInSeconds = 60
```

This variable is to set the heartbeat interval for the subscribed channel for presence before expiry. In the example, we attempt to  that subsciber can expire after 63 seconds if no heartbeat request is received by server.


```c#
_enableResumeOnReconnect = true
```

This variable default value is set to true for retry subscribe and presence to use last timetoken(before this with timetoken = 0). If the variable is set to false, retry subscribe and presence will use timetoken = 0.

```c#
pubnubEnableProxyConfig = false
```

This variable default value is set to false assuming Pubnub code don't need internet access through proxy. If proxy access is needed due to corporate policy, set the value of "pubnubEnableProxyConfig" to true and set the Pubnub property "Proxy" to the type PubnubProxy similar to the following code snippet.
          PubnubProxy proxy = new PubnubProxy();
          proxy.ProxyServer = <<Proxy Host or Server Name>>;
          proxy.ProxyPort = <<Proxy Port Number>>;
          proxy.ProxyUserName = << User Name of the proxy server account holder >>;
          proxy.ProxyPassword = << Password of the proxy server account holder >>;
          pubnub.Proxy = proxy;
At this time, Proxy feature is not supported for windows phone 7.1.

```c#
pubnubLogLevel = LoggingMethod.Level.Off
```

This variable default value is set to LoggingMethod.Level.Off. This is used to log any trace/error message that occur in the application. Other available log level options are LoggingMethod.Level.Error, LoggingMethod.Level.Info, LoggingMethod.Level.Verbose and LoggingMethod.Level.Warning. This variable is for troubleshooting purposes only.

```c#
errorLevel = PubnubErrorFilter.Level.Info
```

This variable default value is set to PubnubErrorFilter.Level.Info. This is used to filter out error messages that go to Error Callback. Other available options are Warning, Critical

```c#
IPubnubUnitTest PubnubUnitTest
```

PubnubUnitTest is a public property which is of type IPubnubUnitTest interface. This property is used to perform unit tests with stubs. The IPubnubUnitTest interface needs to be implemented and passed to PubnubUnitTest property.

```c#
IJsonPluggableLibrary JsonPluggableLibrary
```

JsonPluggableLibrary is a public property which is of type IJsonPluggableLibrary interface. This property is used to customize the JSON library usage within Pubnub API.

```c#
_enableJsonEncodingForPublish = true
```

This variable default value is set to true. It can be set to false only when already serialized string is published as message for Publish.

```c#
string AuthenticationKey
```

AuthenticationKey is a public property used to set authentication key for PAM.

```c#
string Origin
```

Origin is a public property used to set PubNub origin. Default value is set to "pubsub.pubnub.com" through _origin. The default value may change for production purposes. Please check with PubNub support on the origin value.

```c#
string SessionUUID
```

SessionUUID is a public property used to set custom UUID for subscribe/presence sessions. If there is no custom value, PubNub by default uses random GUID value.

```c#
domainName = "pubsub.pubnub.com"
```

This variable default value is set to "pubsub.pubnub.com". This variable is used only for mono runtime. This variable value will be same as "_origin"

** NOTE: ** Some variable values needs to be passed to Pubnub constructor for setup and configuration of PubNub C# API. The variables that needs to be setup are publishKey, subscribeKey, secretKey, cipherKey.

# Please direct all Support Questions and Concerns to Support@PubNub.com
