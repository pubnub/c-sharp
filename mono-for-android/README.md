# PubNub 3.4 Web Data Push Cloud-Hosted API - C# Mono 2.10.9 
##PubNub C Sharp (MonoForAndroid Usage)

You can checkout the video overview here: https://vimeo.com/56720927

Open 3.4/PubNub-Messaging/Pubnub_Messaging/PubNub_Messaging.csproj. 
Run the project in the emulator to see a working example. The main functionality lies in the pubnub.cs file.

3.4/PubNub-Messaging/Andr.Unit contains the Unit test cases. Run the project to see the unit test results,

Please ensure that in order to run on Mono the constant in the pubnub.cs file should be set to "true"
overrideTcpKeepAlive = true;

When creating a new project or a new configuration please add a compiler flag by going into the "Options -> Compiler -> Define Symbols" and adding "MONODROID;" to it.

You can use either Newtonsoft.Json (recommended) or JsonFx as the serialization library. The example project has the references for both Newtonsoft.Json and JsonFx. You can retain either one.

To use JsonFx as the serialization library you need to use the pre-compiler directive USE_JSONFX and REMOVE the reference of Newtonsoft.Json from the project
Limitations of JsonFx: JsonFx doesn't support the serialization for type XmlDocument.

To use Newtonsoft.Json you need not specify any pre-compiler directive. This is the used as the default serialization library. You need to REMOVE the reference of JsonFx from the project and retain the reference of Newtonsoft.Json
Limitations of Newtonsoft.Json: Newtonsoft.Json doesn't support the serialization for type XmlDocument on this platform.

Inbuilt .net serialization is activated by using the pre-compiler directive USE_DOTNET_SERIALIZATION. The use of this library is NOT recommended for pubnub client created on mono platform as this causes issues in "talking" to pubub clients developed for other platforms.

Dev environment setup:
- MAC OS X 10.7.5 (Lion)
- Xamarin.Android 4.6.7
- Xamarin Studio 4.0.8
- Mono 2.10.12 

Report an issue, or email us at support if there are any additional questions or comments.


