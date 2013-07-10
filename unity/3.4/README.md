## PubNub 3.4 Web Data Push Cloud-Hosted API
### Unity 4 for "PC, Mac& Linux Standalone", iOS and "Android" platforms 

### View this First!
We've made a [screencast](https://vimeo.com/69591819) that will walk you through the general setup. Check it out!

### Important Change from previous version
1. Error Callback parameter is being introducted in all operation/non-operation methods of C# Core Pubnub.cs file. If you have previous version, your application might break due to signature difference. Please consider handling Error Callback

### Cheatsheet to migrate to the current version containing Error Callback
Example:
```
Previous => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage);

Migration => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);

NOTE: The callback methods DisplayReturnMessage, DisplayConnectStatusMessage and DisplayErrorMessage(new callback parameter) are used in example code for your review.

```


#### Prerequisites
1. Install Free Full version of Unity 4 Pro from http://unity3d.com/unity/download/ (Unity 4 is recommended, but current/later versions should be ok). MonoDevelop IDE tool will be installed as part of Unity to write C# scripts.
2. In case of unity we need to use JSONFX as the serialization library. To use JSONFX we have defined a pre-compiler variable USE_JSONFX. The other serialization libraries used in the pubnub.cs class are the default serialization from inbuilt .NET class (activated when the pre-compiler directive USE_DOTNET_SERIALIZATION is used) and Newtonsoft.Json (activated when neither USE_JSONFX nor USE_DOTNET_SERIALIZATION is defined). Both of these libraries won't work with UNITY. So you need to retain the pre-compiler variable USE_JSONFX is "defined" at the top of the pubnub.cs class (default behavior).
3. The unit tests for unity also require JSONFX library for serialization.

#### To run the unit test code (under UnityUnitTest folder of Standalone), in addition to the above step, you need to 
1. Import UUnit unity package from http://uunit.googlecode.com/files/UUnit_0.4.unitypackage into your Assets (Updates on this pacakage can be obtained from http://wiki.unity3d.com/index.php?title=UUnit). After import, please delete TestCaseDummy.cs and TestCaseTest.cs files from /Assets/Standard Assets/UUnit/UUnitSelfTest folder to avoid unintended test case runs.
   

#### Running the Demo App on PC

1. Open up the Unity Project from Standalone\PubnubUnity folder present in the PubNub github repo.
2. Ensure "PubNub Example" is added as component to Main Camera.
3. Go to Edit menu --> Project Settings --> Player. 
   Under "Optimization" section, ensure Api Compatibility Level is ".Net 2.0".
4. Go to File menu --> Build Settings --> Select "PC, Linux Standalone" or "Android" Platform.
   Click "Switch Platform" if it is enabled. (NOTE: It will be enabled if there is change in Platform settings)
5. Click Build and Run. (NOTE: To run the demo on "PC, Mac& Linux Standalone" platform, Unity Pro version is required)

#### Running the Demo App on Mac
####Common Steps:

1. Open up the Unity Project from Standalone\PubnubUnity folder present in the PubNub github repo.
2. Ensure "Pubnub Example" is added as component to Main Camera.
3. Goto File menu -> Build Settings.

##### Mac 

1. Follow the 3 steps listed under Common Steps for Running the Demo App on Mac.
2. In the dialog that opens select PC, Mac, Linux Standalone under the platform and click "Switch Platform".
3. On the right side of the same dialog select Mac under "Target Platform".
4. Click "Build and Run"
5. This will run the PubNub example in the unity standalone player for Mac

##### iOS (requires Xcode to be installed)

1. Follow the 3 steps listed under Common Steps for Running the Demo App on Mac.
2. In the dialog the opens select iOS under the platform and click "Switch Platform".
3. Click "Build and Run"
4. This will run the PubNub example in the iPad simulator
5. You may get an error SystemException: System.Net.Sockets are supported only on Unity Pro. Referenced from assembly 'Assembly-CSharp'. If so you need to use the Unity3d pro.
6. The code uses the pre-compiler flag UNITY_IOS to distinguish between other platforms.
7. Target iOS version 4.0 and above. 

##### Android (requires Android development environment to be set)

1. Follow the 3 steps listed under Common Steps for Running the Demo App on Mac.
2. In the dialog the opens select Android under the platform and click "Switch Platform".
3. Click "Build and Run". This button may be disabled or you may get an error that no devices are found. To resolve this please ensure that the android emulator is running.
4. This will run the PubNub example in the Android emulator. 
5. You may get an error SystemException: System.Net.Sockets are supported only on Unity Pro. Referenced from assembly 'Assembly-CSharp'. If so you need to use the Unity3d pro.
6. The code uses the pre-compiler flag UNITY_ANDROID to distinguish between other platforms.
7. If running on the emulator please enable GPU emulation.
8. Target Android version 4 (Ice cream sandwich) and above.


#### Running the Tests

1. Open up the Unity Project from Standalone\UnityUnitTest folder.
2. Click "UUnit" menu to run unit tests. NOTE: If "UUnit" menu item is not showing up, ensure that /Assets/Standard Assets/Editor/UUnit/UUnitTestRunner.cs file exists.
3. Make Console tab as active tab and then Click UUnit so that unit test results will be visible in console.

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
