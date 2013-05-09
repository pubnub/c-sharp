# PubNub 3.4 Web Data Push Cloud-Hosted API
# Unity 4 for Standalone and Web Player

# To run the example/unit test code, you need
1. Intall Free Full version of Unity 4 from http://unity3d.com/unity/download/ (Unity 4 is recommended, but current/later versions should be ok). MonoDevelop IDE tool will be installed as part of Unity to write C# scripts.

# To run the unit test code (under UnityUnitTest folder of Standalone and WebPlayer), in addition to the above step, you need to 
1. Import UUnit unity package from http://uunit.googlecode.com/files/UUnit_0.4.unitypackage into your Assets (Updates on this pacakage can be obtained from http://wiki.unity3d.com/index.php?title=UUnit). Since it was already imported into UnityUnitTest project, no need to import it now.
   

## Running the Demo App

1. Open up the Unity Project from either Standalone\PubnubUnity or WebPlayer\PubnubUnity folder.
2. Ensure "Pubnub Example" is added as component to Main Camera.
3. If running from Standalone\PubnubUnity, Go to Edit menu --> Project Settings --> Player. 
   Under "Optimization" section, ensure Api Compatibility Level is ".Net 2.0".
4. Click the Play button symbol in Game view of toolbar.(You will see Play/Pause/Step buttons).
5. Ensure "Maximize on Play" is selected to view the full game scene of the demo example.

NOTE: WebPlayer Assets\Newtonsoft.Json.dll version is different from Standalone version due to compatibility issues.
Web Player DLL version was downloaded from Unity forum site. http://forum.unity3d.com/threads/50998-Newtonsoft-JSON-NET-Converter-fails-in-Unity

## Running the Tests

1. Open up the Unity Project from either Standalone\UnityUnitTest or WebPlayer\UnityUnitTest folder.
2. "UUnit" menu will be visible to run unit tests. This menu item was visible due to already UUnit unity package.
3. Make Console tab as active tab and then Click UUnit so that unit test results will be visible in console.


NOTE: "/" (slash) being part of channel name will not work. So it is recommeded to avoid using "/" in channel names.

Report an issue, or email us at support if there are any additional questions or comments.
