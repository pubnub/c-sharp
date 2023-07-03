using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace PubnubUtils {
	public class PubNubPackageExporter : MonoBehaviour {
		private static readonly string sourcePath = "Assets/PubNub";
		private static readonly string targetPath = "Packages/com.pubnub.sdk";

		static void CleanUp() {
			if (System.IO.Directory.Exists(targetPath)) {
				System.IO.Directory.Delete(targetPath, true);
			}
		}
		
		[MenuItem("Assets/Export PubNub Package")]
		public static async void ExportPNPackage() {
			CleanUp();
			CopyFilesRecursively(sourcePath, targetPath);

			var assets = new[] { "Packages/com.pubnub.sdk" };

			Debug.Log("Assets to be exported:\n" + string.Join(", ", assets));

			var exportMethod = Assembly.Load("asset-store-tools-editor")
				.GetType("AssetStoreTools.Uploader.PackageExporter")
				.GetMethod("ExportPackage", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			var task = exportMethod.Invoke(
				null,
				new object[] {
					assets,
					"PubNub.unitypackage",
					false,
					false,
					false,
					null
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
			
			CleanUp();
		}
		
		private static void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}
	}
}