using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetStoreTools.Uploader {
public class PubNubPackageExporter : MonoBehaviour {

	[MenuItem("Assets/Export PubNub Package")]
	public static async void ExportPNPackage() {

		// var assets = AssetDatabase.FindAssets("", new[] { "Assets/PubNub" }).Select(AssetDatabase.GUIDToAssetPath)
		// 	.Where(assetFilter).Append("Assets/ScriptTemplates").ToArray();

		var assets = new[] { "Assets/PubNub", "Assets/ScriptTemplates" };

		Debug.Log("Assets to be exported:\n" + string.Join(", ", assets));

		var res = await PackageExporter.ExportPackage(assets, "PubNub.unitypackage", true, false, false, new[] {"com.unity.nuget.newtonsoft-json"});
		Debug.Assert(res, res.Error.Message);
	}
}
}