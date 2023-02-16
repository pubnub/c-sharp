using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace PubnubApi.Unity.Internal.EditorTools {
	[CustomEditor(typeof(PNConfigAsset))]
	public class PNConfigAssetEditor : Editor {
		private readonly string[] propNames = new[] {
			"PublishKey",
			"SubscribeKey",
			"AuthKey",
			"CipherKey",
			"EnableTelemetry",
			"Secure",
			"LogVerbosity"
		};

		private IEnumerable<SerializedProperty> props;

		public override void OnInspectorGUI() {
			props ??= propNames.Select(p => serializedObject.FindProperty(p));

			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextField(new GUIContent("User ID"), "");
			EditorGUI.EndDisabledGroup();
			
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Note that you need to set the UserId variable on runtime, before passing the configuration object to the PubNub instance.", MessageType.Info);
			EditorGUILayout.Space();

			foreach (var prop in props) {
				EditorGUILayout.PropertyField(prop);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}