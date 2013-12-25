## PubNub 3.4 Web Data Push Cloud-Hosted API for Unity 4
### This read me is for Unity-iOS client. For other platforms please refer to folder named  Unity Standalone, Android, PC, Linux

### View this First!
We've made a [screencast](https://vimeo.com/69591819) that will walk you through the general setup. 
After checking out the general setup video, [For iOS targets](https://vimeo.com/71549964) be sure to view this walkthrough next. Check it out!

### Important Change from previous version
Error Callback parameter is being introduced in all operation/non-operation methods of C# Core Pubnub.cs file. 
If you had been using a previous version, your application might break due to signature difference.

#### Cheatsheet to migrate to the new Error Callback implementation

```
Old => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage);

New (current) => pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);

NOTE: The callback methods DisplayReturnMessage, DisplayConnectStatusMessage and DisplayErrorMessage (new callback parameter) are used in the example code for your review.

```

#### Prerequisites
1. Install a free Full version of Unity 4 Pro from http://unity3d.com/unity/download/ (Unity 4 is recommended, but current/later versions should be ok). MonoDevelop IDE tool will be installed as part of Unity to write C# scripts.
2. For Unity on iOS, we have tested 2 JSON serializers
2.1 JsonFx 1.4 mod by TowerOBricks (https://bitbucket.org/TowerOfBricks/jsonfx-for-unity3d/overview). This is recommended and is added by default. If you are finalizing  this library you can delete the class "MiniJson.cs" from the "Assets" directory, and open the project in Unity IDE. Unity IDE will do the rest.
2.2 MiniJson: A small class, with no additional dependencies. But this class can only serialize arrays, strings, Chars, hashtables, Dictionary, arraylist and some more but not custom classes and objects. If you want to use this you need to 
- open the project in MonoDevelop and remove the reference of JsonFx.Json.dll from the references. Delete the JsonFx.Json.dll from the assets folder.
- add MiniJson to the project. This can be done by going to the Solution Explorer, ctrl-clicking Assets -> Add -> Add Files and select the file MiniJson.cs from the Assets folder.
- In the pubnub.cs file uncomment the line which says #define USE_MiniJSON and comment the line #define USE_JSONFX_FOR_UNITY
```
#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_ANDROID)
#define USE_JSONFX
#elif (UNITY_IOS)
#define USE_MiniJSON 
//#define USE_JSONFX_FOR_UNITY
#endif
```

#### To run the unit test code (under UnityUnitTest folder of Standalone), in addition to the above step, you need to 
1. Import UUnit unity package from http://uunit.googlecode.com/files/UUnit_0.4.unitypackage into your Assets (Updates on this pacakage can be obtained from http://wiki.unity3d.com/index.php?title=UUnit). After import, please delete TestCaseDummy.cs and TestCaseTest.cs files from /Assets/Standard Assets/UUnit/UUnitSelfTest folder to avoid unintended test case runs.
   
#### iOS (requires Xcode to be installed)

1. Open up the Unity Project from Standalone\PubnubUnity folder present in the PubNub github repo.
2. Ensure "Pubnub Example" is added as component to Main Camera.
3. Goto File menu -> Build Settings.
4. In the dialog the opens select iOS under the platform and click "Switch Platform".
5. Click "Build and Run"
6. This will run the PubNub example in the iPad simulator
7. You may get an error SystemException: System.Net.Sockets are supported only on Unity Pro. Referenced from assembly 'Assembly-CSharp'. If so you need to use the Unity3d pro.
8. The code uses the pre-compiler flag UNITY_IOS to distinguish between other platforms.
9. Target iOS version 4.0 and above. 

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
