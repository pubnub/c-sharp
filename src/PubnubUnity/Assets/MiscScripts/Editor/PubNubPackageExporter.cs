using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Threading.Tasks;

namespace PubnubUtils {
	public class PubNubPackageExporter : MonoBehaviour {
		[MenuItem("Assets/Export PubNub Package")]
		public static async void ExportPNPackage() {
			var assets = new[] { "Assets/PubNub/UnitySDK" };

			Debug.Log("Assets to be exported:\n" + string.Join(", ", assets));

			var exportMethod = Assembly.Load("asset-store-tools-editor")
				.GetType("AssetStoreTools.Uploader.PackageExporter")
				.GetMethod("ExportPackage", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			var task = exportMethod.Invoke(
				null,
				new object[] {
					assets,
					"PubNub.unitypackage",
					true,
					false,
					false,
					new[] { "com.unity.nuget.newtonsoft-json" }
				}
			) as Task;

			await task;
			var o = task
				.GetType()
				.GetProperty("Result", BindingFlags.Instance | BindingFlags.Public)
				.GetValue(task);
			var r = o.GetType()
				.GetField("Success")
				.GetValue(o) as bool?;

			Debug.Assert(r.Value, "Export broke.");
		}
	}
}