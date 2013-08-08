# PubNub 3.4 Web Data Push Cloud-Hosted API - C# Mono 2.10.9 
##PubNub C Sharp (Mono for Mac) Usage

For a quick video walkthrough, checkout https://vimeo.com/54805916 !

Open 3.4/PubNub-Messaging/PubNub-Console/PubNub-Messaging.csproj, the example Pubnub_Example.cs should demonstrate all functionality, asyncronously using delegates. The main functionality lies in the pubnub.cs file.

3.4/PubNub-Messaging/PubNubTest contains the Unit test cases.

Please ensure that in order to run on Mono the constant in the pubnub.cs file should be set to "true"
overrideTcpKeepAlive = true;

You can use either Newtonsoft.Json (recommended) or JsonFx as the serialization library. The example project has the references for both Newtonsoft.Json and JsonFx. You can retain either one.

To use JsonFx as the serialization library you need to use the pre-compiler directive USE_JSONFX and REMOVE the reference of Newtonsoft.Json from the project
Limitations of JsonFx: JsonFx doesn't support the serialization for type XmlDocument.

To use Newtonsoft.Json you need not specify any pre-compiler directive. This is the used as the default serialization library. You need to REMOVE the reference of JsonFx from the project and retain the reference of Newtonsoft.Json

System.Runtime.Serialization.Json/System.Web.Script.Serialization libraries are activated by using the pre-compiler directive USE_DOTNET_SERIALIZATION. The use of this library is NOT recommended for pubnub client created on mono platform as this causes issues in "talking" to pubnub clients developed for other platforms.

Dev environment setup:
- MAC OS X 10.7.5 (Lion)
- Xamarin Studio 4.0.8
- Xcode 4.6.1
- Mono 2.10.12 

Report an issue, or email us at support if there are any additional questions or comments.


