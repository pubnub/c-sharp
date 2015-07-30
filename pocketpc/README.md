# PubNub 3.7 Web Data Push Cloud-Hosted API
# Pocket PC /Windows CE SDK

Open PubNub-Messaging\PubNub-Messaging.sln
Run the project in "Pocket PC 2003 SE Square Emulator" to see a working example targeting .Net Compact Framework 2.0. The main core functionality lies in the PubNub-Messaging.csproj project. 

## Requirements
1. Windows XP OS or Windows XP Mode on Windows 7
2. Visual Studio 2008
3. Windows Mobile 6 Professional SDK Refresh
4. .NET Compact Framework 2.0 SP1

## Third party software/source code used
1. Newtonsoft.Json.Compact library code
2. The Bouncy Castle Cryptographic C# API
3. ConcurrentHashtable (TvdP.Collections)
4. NUnitLiteCF

### Object Cleanup

For best performance after completion of all intended operations, please call the EndPendingRequests() method of the Pubnub instance, and assign it to null. This will help ensure speedy resources cleanup when you are done with the object.


## Running the emulator
1. To run Pocket PC / Windows CE SDK, you need Windows XP OS with Visual Studio 2008.
2. From Device Emulator Manager, right click on "Pocket PC 2003 SE Square Emulator" and connect. Once connected, right click and select "Cradle". A "New Partndership" windows will popup and select "Guest Partnership" and click next for connet.
3. ActiveSync window displays "Connected" message.
4. Ensure that the date and time is correct by going to emulator Settings -> System -> Clock & Alarms. This is very important for PAM feature.

## Running the Demo/Example App

1. Open up the solution file and select "Pocket PC 2003 SE Square Emulator" as target device.
2. Right click on PubNubMessagingExample project, and set as Startup Project
3. Build the project. Please make sure you compile the whole solution because third party libraries are added as project reference.
4. CTRL-F5 or F5 to run it.
5. Demo will run on Pocket PC emulator

## Running the Tests

1. Open up the solution file and select "Pocket PC 2003 SE Square Emulator" as target device
2. Right click on PubNub-Messaging.Tests project, and set as Startup Project
3. Build the project. Please make sure you compile the whole solution because third party libraries are added as project reference.
4. CTRL-F5 or F5 to run it.
5. Unit tests will run on Pocket PC emulator

## Known issues
1. Occasionally due to concurrent web requests, the JSON response of one web request may come as response of other web request. A work around fix is in place to handle the known unexpected JSON response. 

Report an issue, or email us at support if there are any additional questions or comments.