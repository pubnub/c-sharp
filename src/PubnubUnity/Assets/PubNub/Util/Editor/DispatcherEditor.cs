using UnityEditor;

namespace PubnubApi.Unity.Internal.EditorTools {
	[CustomEditor(typeof(Dispatcher))]
	public class DispatcherEditor : Editor {
		public override void OnInspectorGUI() {
			EditorGUILayout.HelpBox("This script allows dispatching to the main Unity render thread", MessageType.Info);
		}
	}
}

