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

### Code snippet to instantiate Pubnub instance

```
//Basic usage for subscribe and publish
Pubnub pubnub = new Pubnub(publishKey="demo", subscribeKey="demo");

//optional secret key to publish.
Pubnub pubnub = new Pubnub(publishKey="demo", subscribeKey="demo", secretKey);

//optional cipher key to encrypt and decrypt the messages. enableSSL is boolean flag to indicate https request or not.
Pubnub pubnub = new Pubnub(publishKey="demo", subscribeKey="demo", secretKey, cipherKey, enableSSL);

```

### Code snippet to grant PAM access for subscribe channel

```
//Grant Access at sub-key level. At sub-key, no channel will be given.
pubnub.GrantAccess<string>("", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

//Grant Access at channel level.
pubnub.GrantAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

//Grant Access at user level. At user level, auth key will be provided.
pubnub.AuthenticationKey = authKey;
pubnub.GrantAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);


```

### Code snippet to grant PAM access for presence channel

```
//Grant Access at sub-key level. At sub-key, no channel will be given.
pubnub.GrantPresenceAccess<string>("", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

//Grant Access at channel level.
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

//Grant Access at user level. At user level, auth key will be provided.
pubnub.AuthenticationKey = authKey;
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=true, write=true, grantTimeLimitInSeconds=60, DisplayReturnMessage, DisplayErrorMessage);

```

### Code snippet to revoke PAM access for subscribe channel

```
//Revoke Access at user level. At user level, auth key will be provided.
pubnub.AuthenticationKey = authKey;
pubnub.GrantAccess<string>(channel="mychannel", read=false, write=false, DisplayReturnMessage, DisplayErrorMessage);

//Revoke Access at channel level. Any auth keys exist for a given channel shall be revoked before revoking access at channel level.
pubnub.GrantAccess<string>(channel="mychannel", read=false, write=false, DisplayReturnMessage, DisplayErrorMessage);


```

### Code snippet to revoke PAM access for presence channel

```
//Revoke Access at user level. At user level, auth key will be provided.
pubnub.AuthenticationKey = authKey;
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=false, write=false, DisplayReturnMessage, DisplayErrorMessage);

//Revoke Access at channel level. Any auth keys exist for a given channel shall be revoked before revoking access at channel level.
pubnub.GrantPresenceAccess<string>(channel="mychannel", read=false, write=false, DisplayReturnMessage, DisplayErrorMessage);


```

### Code snippet to audit PAM access for subscribe channels

```
//Audit Access at sub-key level. At sub-key, no channel will be given. Here both subscribe and presence channels will be listed.
pubnub.AuditAccess<string>("",DisplayReturnMessage, DisplayErrorMessage);

//Audit Access at channel level.
pubnub.AuditAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);

//Audit Access at user level. At user level, auth key will be provided.
pubnub.AuthenticationKey = authKey;
pubnub.AuditAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);


```

### Code snippet to audit PAM access for presence channels

```
//Audit Access at sub-key level. At sub-key, no channel will be given. Here both subscribe and presence channels will be listed.
pubnub.AuditPresenceAccess<string>("",DisplayReturnMessage, DisplayErrorMessage);

//Audit Access at channel level.
pubnub.AuditPresenceAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);

//Audit Access at user level. At user level, auth key will be provided.
pubnub.AuthenticationKey = authKey;
pubnub.AuditPresenceAccess<string>(channel="mychannel",DisplayReturnMessage, DisplayErrorMessage);

```

### Code snippet to subscribe to a channel

```
pubnub.Subscribe<string>(channel="mychannel", DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);

```

### Code snippet to subscribe to presence channel

```
pubnub.Presence<string>(channel="mychannel", DisplayPresenceReturnMessage, DisplayPresenceConnectStatusMessage, DisplayErrorMessage);

```

### Code snippet to publish a message

```
pubnub.Publish<string>(channel="mychannel", publishMsg="My favorite message", DisplayReturnMessage, DisplayErrorMessage);

```

### Code snippet for detailed history

```
//Detailed History for previously published messages. Max no. of records at a time we can get = 100
pubnub.DetailedHistory<string>(channel="mychannel", recordCountToRetrieve=100, DisplayReturnMessage, DisplayErrorMessage);

//Detailed History from a specific time old to new messages. Max no. of records we can get = 100
pubnub.DetailedHistory<string>(pubnubChannel, starttime=13557486057035336, PollResultsRegularCallback, PollResultsErrorCallback, reverse=true);

```

### Code snippet to HereNow

```
pubnub.HereNow<string>(channel="mychannel", DisplayReturnMessage, DisplayErrorMessage);

```

### Code snippet to unsubscribe a channel

```
pubnub.Unsubscribe<string>(channel="mychannel", DisplayReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage, DisplayErrorMessage);

```

### Code snippet to presence-unsubscribe a channel

```
pubnub.PresenceUnsubscribe<string>(channel="mychannel", DisplayReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage, DisplayErrorMessage);

```


### Code snippet for time

```
pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);

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
