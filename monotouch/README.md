# PubNub 3.4 Web Data Push Cloud-Hosted API - C# Mono 2.10.9 
##PubNub C Sharp (MonoTouch Usage)

For a quick video walkthrough, checkout https://vimeo.com/55630516 !

Open 3.4/PubNub-Messaging/Pubnub-Messaging/PubNub-Messaging.csproj. Run the project in the simulator to see a working example. The main functionality lies in the pubnub.cs file.

3.4/PubNub-Messaging/Pubnub-Messaging.Tests contains the Unit test cases. Run the project to see the unit test results,

Please ensure that in order to run on Mono the constant in the pubnub.cs file should be set to "true"
overrideTcpKeepAlive = true;

When creating a new project or a new configuration please add a compiler flag by going into the "Options -> Compiler -> Define Symbols" and adding "MONOTOUCH;" to it.

You can only use Newtonsoft.Json as the serialization library. The other serialization libraries (JsonFx and the inbuilt serialization library) are not compatible with MonoTouch (Xamarin.iOS)
Limitations of Newtonsoft.Json: Newtonsoft.Json doesn't support the serialization for type XmlDocument on this platform.

Dev environment setup:
- MAC OS X 10.7.5 (Lion)
- Xamarin.iOS 6.2.6.6
- Xamarin Studio 4.0.8
- Xcode 4.6.1
- Mono 2.10.12 

Report an issue, or email us at support if there are any additional questions or comments.


