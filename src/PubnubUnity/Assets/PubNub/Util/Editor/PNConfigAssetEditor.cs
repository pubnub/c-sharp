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

		private SerializedProperty externalJsonEnabled;
		private SerializedProperty externalJsonFile;

		private IEnumerable<SerializedProperty> props;

		private void OnEnable() {
			externalJsonEnabled = serializedObject.FindProperty("externalJsonEnabled");
			externalJsonFile = serializedObject.FindProperty("externalJsonFile");
		}

		public override void OnInspectorGUI() {
			props ??= propNames.Select(p => serializedObject.FindProperty(p));

			serializedObject.Update();
			
			// external file handling
			
			EditorGUILayout.BeginVertical("helpbox");
			EditorGUILayout.PropertyField(externalJsonEnabled,
				new GUIContent("Use external key config"));
			ExternalFileGui();
			EditorGUILayout.EndVertical();
		

			// UserId info
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Note that you need to set the UserId variable on runtime, before passing the configuration object to the PubNub instance.", MessageType.Warning);
			EditorGUILayout.Space();
			
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextField(new GUIContent("User ID"), "");
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			
			// props
			foreach (var prop in props) {
				EditorGUILayout.PropertyField(prop);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void ExternalFileGui() {
			if (!externalJsonEnabled.boolValue || targets.Length > 1) {
				return;
			}

			externalJsonFile.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("External keyset file"), externalJsonFile.objectReferenceValue, typeof(TextAsset), target);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Browse")) {
				EditorGUIUtility.ShowObjectPicker<TextAsset>(externalJsonFile.objectReferenceValue, false, "ext:json", EditorGUIUtility.GetObjectPickerControlID());
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}