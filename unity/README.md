## PubNub 3.6 Web Data Push Cloud-Hosted API for Unity 4
### Supports PC, Mac, Linux, iOS and Android

### View this First!
We've made a [screencast](https://vimeo.com/69591819) that will walk you through the general setup. 
After checking out the general setup video, [For iOS targets](https://vimeo.com/71549964) be sure to view this walkthrough next. Check it out!

### Important changes from previous version
- We have removed the separate versions for Unity, and made this as a common version which  works on all unity distros. The update is an optimized version of the code which was used for Unity iOS. This version uses the JsonFx 1.4 library (mod by TowerOBricks, https://bitbucket.org/TowerOfBricks/jsonfx-for-unity3d/overview).  
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

Report an issue, or email us at support if there are any additional questions or comments.
