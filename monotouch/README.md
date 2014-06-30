# PubNub 3.6 Web Data Push Cloud-Hosted API - C# Mono 3.2.5
##PubNub C Sharp (MonoTouch Usage)

For a quick video walkthrough, checkout https://vimeo.com/55630516 !

### Important Change from previous version
- UserState method parameters have been modified.
- PAM auth method parameters have been modified.
- Implements the features of Pubnub 3.6

### Instructions
Open Pubnub-Messaging/PubNub-Messaging.csproj. Run the project in the simulator to see a working example. The main functionality lies in the pubnub.cs file.

Pubnub-Messaging.Tests contains the Unit test cases. Run the project to see the unit test results,

Please ensure that in order to run on Mono the constant in the pubnub.cs file should be set to "true"
overrideTcpKeepAlive = true;

When creating a new project or a new configuration please add a compiler flag by going into the "Options -> Compiler -> Define Symbols" and adding "MONOTOUCH;" to it.

You can only use Newtonsoft.Json as the serialization library, JsonFx and System.Runtime.Serialization.Json/System.Web.Script.Serialization libraries are not supported. The other serialization libraries (JsonFx and the inbuilt serialization library) are not compatible with MonoTouch (Xamarin.iOS)
Limitations of Newtonsoft.Json: Newtonsoft.Json doesn't support the serialization for type XmlDocument on this platform. 

Dev environment setup:
- MAC OS X 10.8.5 
- Xamarin.iOS 7.0.4.209
- Xamarin Studio 4.2.2
- Xcode 5.0.2
- Mono 3.2.5
- iOS 7

Report an issue, or email us at support if there are any additional questions or comments.


