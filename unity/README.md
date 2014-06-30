## PubNub 3.6 Web Data Push Cloud-Hosted API for Unity 4
### Supports PC, Mac, Linux, iOS and Android

### View this First!
We've made a [screencast](https://vimeo.com/69591819) that will walk you through the general setup. 
After checking out the general setup video, [For iOS targets](https://vimeo.com/71549964) be sure to view this walkthrough next. Check it out!

#### Why do we have 4 versions of Unity code
The PubNub classes which reside under the Assets → Pubnub are the same for all. The code under this folder (along with a ref to a JSON serialization library) is all you need to implement PubNub in your project.

For Unity-iOS and Unity-Standalone/Unity-Android the difference lies in the JSONFX serialization libraries (for iOS and Android/Standalone) making the .csproj files different for these 2 client. The normal version of JsonFx has trouble running under Unity-iOS so we had to use a different library.

Similarly for windows the code is the same but the same project file (.csproj) ( if we used the Android/Standalone’s project file ) was giving issues when opened under windows environment. In order to not run into issues in this case we had to provide a different project. The serialization library used in Windows is the same as the one used in Android/Standalone.   

There are 2 versions of Unity-iOS code (iOS and iOS-MiniJson). The code under the folder iOS uses the JsonFx 1.4 library (mod by TowerOBricks) and the code under iOS-MiniJson used MiniJson as the serialization library.

### Important changes from previous version
- UserState method parameters have been modified.
- PAM auth method parameters have been modified.
- Implements the features of Pubnub 3.6
- Error Callback parameter is being introduced in all operation/non-operation methods of - C# Core Pubnub.cs file. 
- If you had been using a previous version, your application might break due to signature difference.
- Removes the dependency of .NET sockets.

WE have modified the JsonFX pre processor directives: Now we have 3: 

- `USE_JSONFX_UNITY` is for `UNITY_STANDALONE` or `UNITY_WEBPLAYER` or `UNITY_ANDROID`

- `USE_JSONFX` for non-unity clients.

- `USE_JSONFX_UNITY_IOS` is Unity IOS

#### Cheatsheet to migrate to the new Error Callback implementation

```
Old => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage);

New (current) => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);

NOTE: The callback methods DisplayReturnMessage, DisplayConnectStatusMessage and DisplayErrorMessage (new callback parameter) are used in the example code for your review.
```

#####REPLACE
```
USE_JSONFX with USE_JSONFX_UNITY and 

USE_JSONFX_FOR_UNITY with USE_JSONFX_UNITY_IOS
```

#### Prerequisites
+ Install a free Full version of Unity 4 Pro from http://unity3d.com/unity/download/ (Unity 4 is recommended, but current/later versions should be ok). MonoDevelop IDE tool will be installed as part of Unity to write C# scripts.
+ For Unity, JSONFX is needed for the serialization library. To use JSONFX we have defined 3 pre processor directives

    * `USE_JSONFX_UNITY` is for `UNITY_STANDALONE` or `UNITY_WEBPLAYER` or `UNITY_ANDROID`

    * `USE_JSONFX` for non-unity clients.

    * `USE_JSONFX_UNITY_IOS` is Unity IOS

Please note the other serialization libraries used in the pubnub.cs class are the default from the builtin .NET class (activated when the pre-compiler directive USE_DOTNET_SERIALIZATION is used) and Newtonsoft.Json (activated when neither USE_JSONFX nor USE_DOTNET_SERIALIZATION is defined). Both of these libraries won't work with UNITY. So you need to be sure the pre-compiler variable USE_JSONFX is "defined" at the top of the pubnub.cs class (default behavior).

#### To run the unit tests, in addition to the above, you need to 
1. Import UnityTestTools package (this is already present in the Pubnub client code under the path PubnubUnity/Assets/UnityTestTools) into your Assets. (https://www.assetstore.unity3d.com/#/content/13802)

#### Running the Demo App on PC

Please use the folder [Windows-Standalone](Windows-Standalone) and refer to the ReadMe in it.

#### Running the Demo App on Mac, Linux and Android

Please use the folder [Mac-Linux-Android-Standalone](Mac-Linux-Android-Standalone) and refer to the ReadMe in it.

#### Running the Demo App on iOS

Please use the folder [iOS](iOS) or [iOS-MiniJson](iOS-MiniJson) and refer to the ReadMe in it.

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

Report an issue, or email us at support if there are any additional questions or comments.
