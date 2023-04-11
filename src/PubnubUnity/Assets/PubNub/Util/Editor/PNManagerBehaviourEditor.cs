using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PubnubApi.Unity.Internal.EditorTools {
	[CustomEditor(typeof(PNManagerBehaviour), true)]
	public class PNManagerBehaviourEditor : Editor {
		private IEnumerable<string> baseFields = new[] { "pnConfiguration" };
		private IEnumerable<string> fields;

		void OnEnable() {
			// support for inheritance
			fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Select(f => f.Name).Except(baseFields);
		}
		
		public override void OnInspectorGUI() {
			serializedObject.Update();

			if (target.GetType() == typeof(PNManagerBehaviour)) {
				EditorGUILayout.HelpBox("To fully utilize PubNub's functionality, you need to extend or reference this component.", MessageType.Info);
				EditorGUILayout.Space();
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("pnConfiguration"), new GUIContent("PubNub Configuration"));

			if (fields.Any()) {
				EditorGUILayout.Space();
			}
			
			// display the inherited fields
			foreach (var field in fields) {
				EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("This component will automatically set the DontDestroyOnLoad flag on Initialize", MessageType.Info);
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();
		}
	}
}