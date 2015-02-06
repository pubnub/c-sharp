# Please direct all Support Questions and Concerns to Support@PubNub.com

## PubNub 3.6 Web Data Push Cloud-Hosted API for Unity 4
### Supports PC, Mac, Linux, iOS and Android

### View this First!
We've made a [screencast](https://vimeo.com/69591819) that will walk you through the general setup. 
After checking out the general setup video, [For iOS targets](https://vimeo.com/71549964) be sure to view this walkthrough next. Check it out!

### Important changes from previous version
- UserState method parameters have been modified.
- PAM auth method parameters have been modified.
- Implements the features of Pubnub 3.6
 - Renamed property name HeartbeatInterval to LocalClientHeartbeatInterval.
 - New properties PresenceHeartbeat, PresenceHeartbeatInterval were added.
 - Additional optional parameters showUUIDList, includeUserState for HereNow method.
 - New methods GlobalHereNow, WhereNow, SetUserState, GetUserState, SetLocalUserState, GetLocalUserState and ChangeUUID
 - UserState data in the response format of presence events.
- We have removed the separate versions for Unity, and made this as a common version which  works on all unity distros. The update is an optimized version of the code which was used for Unity iOS. This version uses the JsonFx 1.4 library (mod by TowerOBricks, https://bitbucket.org/TowerOfBricks/jsonfx-for-unity3d/overview).  
- Now the SDK uses only one pre-processor directive for all Unity distros
i.e.`USE_JSONFX_UNITY_IOS`

#### Changes in the earlier versions
- UserState method parameters have been modified.
- PAM auth method parameters have been modified.
- Implements the features of Pubnub 3.6
- Error Callback parameter is being introduced in all operation/non-operation methods of - C# Core Pubnub.cs file. 
- If you had been using a previous version, your application might break due to signature difference.
- Removes the dependency of .NET sockets.

#### Cheatsheet to migrate to the new Error Callback implementation


#####REPLACE
```
USE_JSONFX with USE_JSONFX_UNITY_IOS 

or 

USE_JSONFX_FOR_UNITY with USE_JSONFX_UNITY_IOS
```

#####REPLACE
```
Old => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage);

New (current) => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);

NOTE: The callback methods DisplayReturnMessage, DisplayConnectStatusMessage and DisplayErrorMessage (new callback parameter) are used in the example code for your review.
```

#### Prerequisites
+ Install a free Full version of Unity 4 Pro from http://unity3d.com/unity/download/ (Unity 4 is recommended, but current/later versions should be ok). MonoDevelop IDE tool will be installed as part of Unity to write C# scripts.
+ For Unity, JSONFX (1.4 mod by TowerOBricks) is needed for the serialization library. To use JSONFX we use the pre-processor directive `USE_JSONFX_UNITY_IOS` which works on all unity distros.

Please note the other serialization libraries used in the pubnubCore.cs and pubnubUnity.cs classes are the default from the builtin .NET class (activated when the pre-compiler directive USE_DOTNET_SERIALIZATION is used), the Newtonsoft.Json (activated when neither USE_JSONFX nor USE_DOTNET_SERIALIZATION is defined) and the latest version JSONFX (activated when the directive USE_JSONFX is defined) are not supported on unity. 

The directive `USE_JSONFX_UNITY` (uses JSONFx 1.4 dll) works only for UNITY_STANDALONE or UNITY_WEBPLAYER or UNITY_ANDROID and NOT for UNITY_IOS. This directive is not recommended to use in this version.

#### To run the unit tests, in addition to the above, you need to 
1. Import UnityTestTools package (this is already present in the Pubnub client code under the path Assets/UnityTestTools) into your Assets. (https://www.assetstore.unity3d.com/#/content/13802)

#### Running the Demo App on Mac, Linux and Android

####Common Steps:

1. Open up the Unity Project 
2. Ensure "PubnubExample" is added as component to Main Camera.
3. Goto File menu -> Build Settings.

##### Mac / PC /Linux

1. Follow the 3 steps listed under Common Steps for Running the Demo App on Mac, Linux and Android.
2. In the dialog that opens select PC, Mac, Linux Standalone under the platform and click "Switch Platform".
3. On the right side of the same dialog select Mac or Windows or Linux under "Target Platform".
4. Click "Build and Run"
5. This will run the PubNub example in the unity standalone player for Mac

##### Android (requires Android development environment to be set)

1. Follow the 3 steps listed under Common Steps for Running the Demo App on Mac.
2. In the dialog the opens select Android under the platform and click "Switch Platform".
3. Click "Build and Run". This button may be disabled or you may get an error that no devices are found. To resolve this please ensure that the android emulator is running.
4. This will run the PubNub example in the Android emulator. 
5. The code uses the pre-compiler flag UNITY_ANDROID to distinguish between other platforms.
6. If running on the emulator please enable GPU emulation.
7. Target Android version 4 (Ice cream sandwich) or above.

#### iOS (requires Xcode to be installed)

1. Open up the Unity Project 
2. Ensure "PubnubExample" is added as component to Main Camera.
3. Goto File menu -> Build Settings.
4. In the dialog the opens select iOS under the platform and click "Switch Platform".
5. Click "Build and Run"
6. This will run the PubNub example in the iPad simulator
7. The code uses the pre-compiler flag UNITY_IOS to distinguish between other platforms.
8. Target iOS version 4.0 and above. 

#### Running the Tests

1. Open up the Unity Project from iOS folder.
2. From the "Unity test tools" menu select the "Unit test runner" option.
3. Run the tests by clicking the "play" button or by selecting the individual tests.

#### Potential Errors and their resolutions:

1) Internal compiler error. See the console log for more information. output was:

Unhandled Exception: System.TypeLoadException: Could not load type 'Newtonsoft.Json.Linq.JContainer' from assembly 'Newtonsoft.Json, Version=4.5.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'

Resolution:

Go To Edit -> Project Settings --> Player --> Inspector tab displays.
In Inspector tab window, Under  Per-Platform settings --> "Settings for PC, Mac & Linux Standalone" or "Settings for Android", Optimization section, Set Api Compatibility Level to .NET 2.0.

Go To File -> Build Settings -> Under Platform select "PC, Mac & Linux Standalone" or "Android". 
Under "PC, Mac & Linux Standalone" section, Set Target Platform to Windows and Architecture to "x86_x64" (for 64 bit OS)


2) Internal compiler error. See the console log for more information. output was:

Unhandled Exception: System.Reflection.ReflectionTypeLoadException: The classes in the module cannot be loaded.


Resolution: This error occurs mostly due to incompatible JSON library. Unity Standalone version is compatible with JSON.net library binary versions targeting .NET Framework 2.0 or 3.5. Please use JSON.NET versions supporting .NET20/.NET35 until future versions support.


3) Internal compiler error. See the console log for more information. output was:
Unhandled Exception: System.TypeLoadException: Could not load type 'System.Runtime.Versioning.TargetFrameworkAttribute' from assembly 'Newtonsoft.Json'

Resolution: Unity Standalone version works only on Windows OS platforms. Current version do not support Unity Android or iOS or any other mobile platform.

4) System.Net.WebException: Error getting response stream (Write: The authentication or decryption has failed.): SendFailure ---> System.IO.IOException: The authentication or decryption has failed. ---> Mono.Security.Protocol.Tls.TlsException: Invalid certificate received from server

Resolution: This error may occur when the Unity example was run with SSL enabled. Please ensure that ValidateServerCertificate method in PubnubExample.cs do not get Untrust root error. For testing, you can return "true" for ValidateServerCertificate method to verify whether error is gone or not. However there is security risk in hardcoding "true" in production environment.

* NOTE: "/" (slash) being part of channel name will not work. So it is recommeded to avoid using "/" in channel names.

### Object Cleanup

For best performance after completion of all intended operations, please call the EndPendingRequests() method
of the Pubnub instance, and assign it to null. This will help ensure speedy resources cleanup when you are done
with the object.

### For the calls listed below (apart from the instantiation of Pubnub instance) it is recommended that you call them on a background thread to avoid some UI freezes on slow connections. e.g.:

```c#
#if(UNITY_IOS || UNITY_ANDROID)
        Thread requestThread = new Thread (delegate (object state) {
            pubnub.Subscribe<string>(channel="mychannel", DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
        }); 
        requestThread.Name = "requestThread";
        requestThread.Start ();
#else
        ThreadPool.QueueUserWorkItem (new WaitCallback (
           pubnub.Subscribe<string>(channel="mychannel", DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
        ), pubnubState);
#endif
```

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
  Display(pubnubError.StatusCode)
  
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

  Display(pubnubError.StatusCode); //Unique ID of the error

  Display(pubnubError.Message); //Message received from client or server. From client, it could be from .NET exception.

  if (pubnubError.DetailedDotNetException != null)
  {
    Display(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
    Display(pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
  }

  Display(pubnubError.MessageSource); // Did this originate from Server or Client-side logic

  if (pubnubError.PubnubWebRequest != null)
  {
    //Captured Web Request details
    Display(pubnubError.PubnubWebRequest.RequestUri.ToString()); 
    Display(pubnubError.PubnubWebRequest.Headers.ToString()); 
  }

  if (pubnubError.PubnubWebResponse != null)
  {
    //Captured Web Response details
    Display(pubnubError.PubnubWebResponse.Headers.ToString());
  }

  Display(pubnubError.Description); // Useful for logging and troubleshooting and support
  Display(pubnubError.Channel); //Channel name(s) at the time of error
  Display(pubnubError.ErrorDateTimeGMT); //GMT time of error

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
        Display("Cool. Messaage Published");
      }
      else
      {
        Display("Oops. Some problem."); 
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
        Display("Now we are good to receive published messages");
      }
    }
  }
}
```

### Display method can something like this: 
```
public void Display (string strText)
        {
            UnityEngine.Debug.Log (string.Format ("CALLBACK LOG: {0}", strText));
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

## Variables Reference

```c#
overrideTcpKeepAlive = true 
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

This variable is to set the wait time for re-subscribe and re-presence, if the previous subscribe or presence fail. 
 
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
```c#

          PubnubProxy proxy = new PubnubProxy();
          proxy.ProxyServer = <<Proxy Host or Server Name>>;
          proxy.ProxyPort = <<Proxy Port Number>>;
          proxy.ProxyUserName = << User Name of the proxy server account holder >>;
          proxy.ProxyPassword = << Password of the proxy server account holder >>;
          pubnub.Proxy = proxy;
```

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

# Please direct all Support Questions and Concerns to Support@PubNub.com
