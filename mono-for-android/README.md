# PubNub 3.6 Web Data Push Cloud-Hosted API - C# Mono 3.2.5
##PubNub C Sharp (MonoForAndroid Usage)

You can checkout the video overview here: https://vimeo.com/56720927

Open Pubnub_Messaging/PubNub_Messaging.csproj. 
Run the project in the emulator to see a working example. The main functionality lies in the pubnub.cs file.

Andr.Unit-master contains the Unit test cases. Run the project to see the unit test results,

Please ensure that in order to run on Mono the constant in the pubnub.cs file should be set to "true"
overrideTcpKeepAlive = true;

When creating a new project or a new configuration please add a compiler flag by going into the "Options -> Compiler -> Define Symbols" and adding "MONODROID;" to it.

You can use either Newtonsoft.Json (recommended) or JsonFx as the serialization library. The example project has the references for both Newtonsoft.Json and JsonFx. You can retain either one.

To use JsonFx as the serialization library you need to use the pre processor directive USE_JSONFX in the files PubnubCore.cs and PubnubXamarinAndroid.cs and REMOVE the reference of Newtonsoft.Json from the project
Please note that JsonFx doesn't support the serialization for type XmlDocument.

To use Newtonsoft.Json you need not specify any pre processor directive. This is the used as the default serialization library. You need to REMOVE the reference of JsonFx from the project and retain the reference of Newtonsoft.Json
Limitations of Newtonsoft.Json: Newtonsoft.Json doesn't support the serialization for type XmlDocument on this platform.

System.Runtime.Serialization.Json/System.Web.Script.Serialization libraries are activated  by using the pre-compiler directive USE_DOTNET_SERIALIZATION. The use of this library is NOT recommended for pubnub client created on mono platform as this causes issues in "talking" to pubnub clients developed for other platforms.

Dev environment setup:
- MAC OS X 10.8.5 
- Xamarin.Android 4.10.1
- Xamarin Studio 4.2.2
- Mono 3.2.5
- Android 4.1

Report an issue, or email us at support if there are any additional questions or comments.


