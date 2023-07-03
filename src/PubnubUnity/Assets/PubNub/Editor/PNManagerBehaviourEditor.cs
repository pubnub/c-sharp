using System;
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

		private IEnumerable<System.Type> implementations = new List<Type>();

		void OnEnable() {
			// support for inheritance
			fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Select(f => f.Name).Except(baseFields);

			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				implementations = implementations.Concat(assembly.GetTypes()
					.Where(t => t.BaseType == typeof(PNManagerBehaviour)));
			}
		}
		
		public override void OnInspectorGUI() {
			serializedObject.Update();

			if (target.GetType() == typeof(PNManagerBehaviour)) {
				EditorGUILayout.HelpBox("To utilize PubNub's functionality, you need to extend this component.", MessageType.Info);
				EditorGUILayout.Space();

				if (implementations.Any()) {
					GUILayout.BeginVertical("box");
					
					EditorGUILayout.LabelField("Replace with an implementation:");
					
					foreach (var impl in implementations) {
						if (GUILayout.Button(impl.Name)) {
							EditorApplication.delayCall += () => {
								var go = (target as PNManagerBehaviour).gameObject;
								go.AddComponent(impl);
								DestroyImmediate(target);
								EditorUtility.SetDirty(go);
							};
						}
					}
					
					GUILayout.EndVertical();
				}
			} else {
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("pnConfiguration"), new GUIContent("PubNub Configuration"));

			if (fields.Any()) {
				EditorGUILayout.Space();
			}
			
			// display the inherited fields
			foreach (var field in fields) {
				EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
			}
			
			if (!EditorApplication.isPlayingOrWillChangePlaymode) {
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("This component will set the DontDestroyOnLoad flag on Initialize",
					MessageType.Info);
				EditorGUILayout.Space();
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}