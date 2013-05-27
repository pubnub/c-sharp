## PubNub 3.4 Web Data Push Cloud-Hosted API
### Unity 4 for Standalone and Web Player

#### Prerequisites
1. Intall Free Full version of Unity 4 from http://unity3d.com/unity/download/ (Unity 4 is recommended, but current/later versions should be ok). MonoDevelop IDE tool will be installed as part of Unity to write C# scripts.

#### To run the unit test code (under UnityUnitTest folder of Standalone and WebPlayer), in addition to the above step, you need to 
1. Import UUnit unity package from http://uunit.googlecode.com/files/UUnit_0.4.unitypackage into your Assets (Updates on this pacakage can be obtained from http://wiki.unity3d.com/index.php?title=UUnit). After import, please delete TestCaseDummy.cs and TestCaseTest.cs files from /Assets/Standard Assets/UUnit/UUnitSelfTest folder to avoid unintendend test case runs.
   

#### Running the Demo App

1. Open up the Unity Project from either Standalone\PubnubUnity or WebPlayer\PubnubUnity folder.
2. Ensure "Pubnub Example" is added as component to Main Camera.
3. If running from Standalone\PubnubUnity, Go to Edit menu --> Project Settings --> Player. 
   Under "Optimization" section, ensure Api Compatibility Level is ".Net 2.0".
4. Click the Play button symbol in Game view of toolbar.(You will see Play/Pause/Step buttons).
5. Ensure "Maximize on Play" is selected to view the full game scene of the demo example.

* NOTE: WebPlayer Assets\Newtonsoft.Json.dll version is different from Standalone version due to compatibility issues.
Web Player DLL version was downloaded from Unity forum site. http://forum.unity3d.com/threads/50998-Newtonsoft-JSON-NET-Converter-fails-in-Unity

#### Running the Tests

1. Open up the Unity Project from either Standalone\UnityUnitTest or WebPlayer\UnityUnitTest folder.
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
